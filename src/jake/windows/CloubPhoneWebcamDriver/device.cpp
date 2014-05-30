#include "avshws.h"
/**************************************************************************

PAGEABLE CODE

**************************************************************************/

#ifdef ALLOC_PRAGMA
#pragma code_seg("PAGE")
#endif // ALLOC_PRAGMA
#define IOCTL_IMAGE	CTL_CODE(FILE_DEVICE_UNKNOWN,0x4000,METHOD_BUFFERED,FILE_ANY_ACCESS)

typedef long(*DispatchFunctionPtr)(IN PDEVICE_OBJECT DeviceObject, IN PIRP Irp);

DispatchFunctionPtr fpClassDispatchfunction;
DispatchFunctionPtr fpClassCreatefunction;
UNICODE_STRING DeviceLink;
UNICODE_STRING DeviceName;

UCHAR psyImageBuf_[320 * 240][3] = { 0, };

/*++

Routine Description:

Create the capture device.  This is the creation dispatch for the
capture device.

Arguments:

Device -
The AVStream device being created.

Return Value:

Success / Failure

--*/
NTSTATUS CCaptureDevice::DispatchCreate(IN PKSDEVICE Device)
{
	PAGED_CODE();
	NTSTATUS Status;
	CCaptureDevice *CapDevice = new (NonPagedPool)CCaptureDevice(Device);

	if (!CapDevice) {
		Status = STATUS_INSUFFICIENT_RESOURCES;
	} else {
		KsAcquireDevice(Device);
		Status = KsAddItemToObjectBag(Device->Bag, reinterpret_cast <PVOID> (CapDevice), reinterpret_cast <PFNKSFREE> (CCaptureDevice::Cleanup));
		KsReleaseDevice(Device);
		if (!NT_SUCCESS(Status)) {
			delete CapDevice;
		}
		else {
			Device->Context = reinterpret_cast <PVOID> (CapDevice);
		}
	}
	return Status;
}

/*************************************************/

/*++

Routine Description:

Called at Pnp start.  We start up our virtual hardware simulation.
f
Arguments:

TranslatedResourceList -
The translated resource list from Pnp

UntranslatedResourceList -
The untranslated resource list from Pnp

Return Value:

Success / Failure

--*/
NTSTATUS CCaptureDevice::PnpStart(IN PCM_RESOURCE_LIST TranslatedResourceList, IN PCM_RESOURCE_LIST UntranslatedResourceList)
{
	PAGED_CODE();

	NTSTATUS Status = STATUS_SUCCESS;

	if (!m_Device->Started) {
		// Create the Filter for the device
		KsAcquireDevice(m_Device);
		Status = KsCreateFilterFactory(m_Device->FunctionalDeviceObject, &CaptureFilterDescriptor, L"GLOBAL", NULL, KSCREATE_ITEM_FREEONSTOP, NULL, NULL, NULL);
		KsReleaseDevice(m_Device);
	}

	if (NT_SUCCESS(Status) && (!m_Device->Started)) {
		m_HardwareSimulation = new (NonPagedPool)CHardwareSimulation(this);
		if (!m_HardwareSimulation) {
			Status = STATUS_INSUFFICIENT_RESOURCES;
		}
		else {
			Status = KsAddItemToObjectBag(m_Device->Bag, reinterpret_cast <PVOID> (m_HardwareSimulation), reinterpret_cast <PFNKSFREE> (CHardwareSimulation::Cleanup));
			if (!NT_SUCCESS(Status)) {
				delete m_HardwareSimulation;
			}
		}
#if defined(_X86_)
		INTERFACE_TYPE InterfaceBuffer;
		ULONG InterfaceLength;
		DEVICE_DESCRIPTION DeviceDescription;

		if (NT_SUCCESS(Status)) {
			IoGetDeviceProperty(m_Device->PhysicalDeviceObject, DevicePropertyLegacyBusType, sizeof (INTERFACE_TYPE), &InterfaceBuffer, &InterfaceLength);

			DeviceDescription.Version = DEVICE_DESCRIPTION_VERSION;
			DeviceDescription.DmaChannel = ((ULONG)~0);
			DeviceDescription.InterfaceType = PCIBus;
			DeviceDescription.DmaWidth = Width32Bits;
			DeviceDescription.DmaSpeed = Compatible;
			DeviceDescription.ScatterGather = TRUE;
			DeviceDescription.Master = TRUE;
			DeviceDescription.Dma32BitAddresses = TRUE;
			DeviceDescription.AutoInitialize = FALSE;
			DeviceDescription.MaximumLength = (ULONG)-1;

			m_DmaAdapterObject = IoGetDmaAdapter(m_Device->PhysicalDeviceObject, &DeviceDescription, &m_NumberOfMapRegisters);

			if (!m_DmaAdapterObject) {
				Status = STATUS_UNSUCCESSFUL;
			}
		}

		if (NT_SUCCESS(Status)) {
			KsDeviceRegisterAdapterObject(m_Device, m_DmaAdapterObject, (1 << 20), sizeof (KSMAPPING));
		}
#endif
	}
	return Status;
}

/*************************************************/

/*++

Routine Description:

This is the pnp stop dispatch for the capture device.  It releases any
adapter object previously allocated by IoGetDmaAdapter during Pnp Start.

Arguments:

None

Return Value:

None

--*/
void CCaptureDevice::PnpStop()
{
	DbgPrint("enter the pnp stop");
	PAGED_CODE();
	if (m_DmaAdapterObject) {
		//
		// Return the DMA adapter back to the system.
		//
		m_DmaAdapterObject->DmaOperations->PutDmaAdapter(m_DmaAdapterObject);
		m_DmaAdapterObject = NULL;
	}
}

/*************************************************/

/*++

Routine Description:

Acquire hardware resources for the capture hardware.  If the
resources are already acquired, this will return an error.
The hardware configuration must be passed as a VideoInfoHeader.

Arguments:

CaptureSink -
The capture sink attempting to acquire resources.  When scatter /
gather mappings are completed, the capture sink specified here is
what is notified of the completions.

VideoInfoHeader -
Information about the capture stream.  This **MUST** remain
stable until the caller releases hardware resources.  Note
that this could also be guaranteed by bagging it in the device
object bag as well.

Return Value:

Success / Failure

--*/

NTSTATUS CCaptureDevice::AcquireHardwareResources(IN ICaptureSink *CaptureSink, IN PKS_VIDEOINFOHEADER VideoInfoHeader)
{
	PAGED_CODE();
	NTSTATUS Status = STATUS_SUCCESS;

	//
	// If we're the first pin to go into acquire (remember we can have
	// a filter in another graph going simultaneously), grab the resources.
	//
	if (InterlockedCompareExchange(&m_PinsWithResources, 1, 0) == 0) {
		m_VideoInfoHeader = VideoInfoHeader;
		//
		// If there's an old hardware simulation sitting around for some
		// reason, blow it away.
		//
		if (m_ImageSynth) {
			delete m_ImageSynth;
			m_ImageSynth = NULL;
		}

		//
		// Create the necessary type of image synthesizer.
		//
		if (m_VideoInfoHeader->bmiHeader.biBitCount == 24 && m_VideoInfoHeader->bmiHeader.biCompression == KS_BI_RGB) {

			//
			// If we're RGB24, create a new RGB24 synth.  RGB24 surfaces
			// can be in either orientation.  The origin is lower left if
			// height < 0.  Otherwise, it's upper left.
			//
			m_ImageSynth = new (NonPagedPool, 'RysI') CRGB24Synthesizer(m_VideoInfoHeader->bmiHeader.biHeight >= 0);
		} else if (m_VideoInfoHeader->bmiHeader.biBitCount == 16 &&
			(m_VideoInfoHeader->bmiHeader.biCompression == FOURCC_YUY2)) {

			//
			// If we're UYVY, create the YUV synth.
			//
			m_ImageSynth = new(NonPagedPool, 'YysI') CYUVSynthesizer;
		} else
			//
			// We don't synthesize anything but RGB 24 and UYVY.
			//
			Status = STATUS_INVALID_PARAMETER;

		if (NT_SUCCESS(Status) && !m_ImageSynth) {
			Status = STATUS_INSUFFICIENT_RESOURCES;
		}

		if (NT_SUCCESS(Status)) {
			//
			// If everything has succeeded thus far, set the capture sink.
			//
			m_CaptureSink = CaptureSink;
		}
		else {
			//
			// If anything failed in here, we release the resources we've
			// acquired.
			//
			ReleaseHardwareResources();
		}

	}
	else {

		//
		// TODO: Better status code?
		//
		Status = STATUS_SHARING_VIOLATION;
	}
	return Status;
}

/*************************************************/

/*++

Routine Description:

Release hardware resources.  This should only be called by
an object which has acquired them.

Arguments:

None

Return Value:

None

--*/
void CCaptureDevice::ReleaseHardwareResources()
{
	PAGED_CODE();
	//
	// Blow away the image synth.
	//
	if (m_ImageSynth) {
		delete m_ImageSynth;
		m_ImageSynth = NULL;
	}
	m_VideoInfoHeader = NULL;
	m_CaptureSink = NULL;

	//
	// Release our "lock" on hardware resources.  This will allow another
	// pin (perhaps in another graph) to acquire them.
	//
	InterlockedExchange(&m_PinsWithResources, 0);
}

/*************************************************/

/*++

Routine Description:

Start the capture device based on the video info header we were told
about when resources were acquired.

Arguments:

None

Return Value:

Success / Failure

--*/
NTSTATUS CCaptureDevice::Start()
{
	PAGED_CODE();
	m_LastMappingsCompleted = 0;
	m_InterruptTime = 0;

	return m_HardwareSimulation->Start(m_ImageSynth, m_VideoInfoHeader->AvgTimePerFrame, m_VideoInfoHeader->bmiHeader.biWidth, ABS(m_VideoInfoHeader->bmiHeader.biHeight), m_VideoInfoHeader->bmiHeader.biSizeImage);
}

/*************************************************/

/*++

Routine Description:

Pause or unpause the hardware simulation.  This is an effective start
or stop without resetting counters and formats.  Note that this can
only be called to transition from started -> paused -> started.  Calling
this without starting the hardware with Start() does nothing.

Arguments:

Pausing -
An indicatation of whether we are pausing or unpausing

TRUE -
Pause the hardware simulation

FALSE -
Unpause the hardware simulation

Return Value:

Success / Failure

--*/
NTSTATUS CCaptureDevice::Pause(IN BOOLEAN Pausing)
{
	PAGED_CODE();
	return m_HardwareSimulation->Pause(Pausing);
}

/*************************************************/

/*++

Routine Description:

Stop the capture device.

Arguments:

None

Return Value:

Success / Failure

--*/
NTSTATUS CCaptureDevice::Stop()
{
	PAGED_CODE();
	return m_HardwareSimulation->Stop();
}

/*************************************************/

/*++

Routine Description:

Program the scatter / gather mappings for the "fake" hardware.

Arguments:

Buffer -
Points to a pointer to the virtual address of the topmost
scatter / gather chunk.  The pointer will be updated as the
device "programs" mappings.  Reason for this is that we get
the physical addresses and sizes, but must calculate the virtual
addresses...  This is used as scratch space for that.

Mappings -
An array of mappings to program

MappingsCount -
The count of mappings in the array

Return Value:

The number of mappings successfully programmed

--*/
ULONG CCaptureDevice::ProgramScatterGatherMappings(IN PUCHAR *Buffer, IN PKSMAPPING Mappings, IN ULONG MappingsCount)
{
	PAGED_CODE();

	return m_HardwareSimulation->ProgramScatterGatherMappings(Buffer, Mappings, MappingsCount, sizeof (KSMAPPING));
}

/*************************************************************************

LOCKED CODE

**************************************************************************/

#ifdef ALLOC_PRAGMA
#pragma code_seg()
#endif // ALLOC_PRAGMA

/*++

Routine Description:

Return the number of frame intervals that have elapsed since the
start of the device.  This will be the frame number.

Arguments:

None

Return Value:

The interrupt time of the device (the number of frame intervals that
have elapsed since the start of the device).

--*/
ULONG CCaptureDevice::QueryInterruptTime()
{
	return m_InterruptTime;
}

/*************************************************/

/*++

Routine Description:

This is the "faked" interrupt service routine for this device.  It
is called at dispatch level by the hardware simulation.

Arguments:

None

Return Value:

None

--*/
void CCaptureDevice::Interrupt()
{
	m_InterruptTime++;

	//
	// Realistically, we'd do some hardware manipulation here and then queue
	// a DPC.  Since this is fake hardware, we do what's necessary here.  This
	// is pretty much what the DPC would look like short of the access
	// of hardware registers (ReadNumberOfMappingsCompleted) which would likely
	// be done in the ISR.
	//
	ULONG NumMappingsCompleted = m_HardwareSimulation->ReadNumberOfMappingsCompleted();

	//
	// Inform the capture sink that a given number of scatter / gather
	// mappings have completed.
	//
	m_CaptureSink->CompleteMappings(NumMappingsCompleted - m_LastMappingsCompleted);
	m_LastMappingsCompleted = NumMappingsCompleted;
}

/**************************************************************************

DESCRIPTOR AND DISPATCH LAYOUT

**************************************************************************/

//
// CaptureFilterDescriptor:
//
// The filter descriptor for the capture device.
DEFINE_KSFILTER_DESCRIPTOR_TABLE(FilterDescriptors) {
	&CaptureFilterDescriptor
};

//
// CaptureDeviceDispatch:
//
// This is the dispatch table for the capture device.  Plug and play
// notifications as well as power management notifications are dispatched
// through this table.
//
const
KSDEVICE_DISPATCH
CaptureDeviceDispatch = {
	CCaptureDevice::DispatchCreate,         // Pnp Add Device
	CCaptureDevice::DispatchPnpStart,       // Pnp Start
	NULL,                                   // Post-Start
	NULL,                                   // Pnp Query Stop
	NULL,                                   // Pnp Cancel Stop
	CCaptureDevice::DispatchPnpStop,        // Pnp Stop
	NULL,                                   // Pnp Query Remove
	NULL,                                   // Pnp Cancel Remove
	NULL,                                   // Pnp Remove
	NULL,                                   // Pnp Query Capabilities
	NULL,                                   // Pnp Surprise Removal
	NULL,                                   // Power Query Power
	NULL,                                   // Power Set Power
	NULL                                    // Pnp Query Interface
};

//
// CaptureDeviceDescriptor:
//
// This is the device descriptor for the capture device.  It points to the
// dispatch table and contains a list of filter descriptors that describe
// filter-types that this device supports.  Note that the filter-descriptors
// can be created dynamically and the factories created via 
// KsCreateFilterFactory as well.  
//
const KSDEVICE_DESCRIPTOR CaptureDeviceDescriptor = { &CaptureDeviceDispatch, 0, NULL };



/**************************************************************************/

#define DEVICE_NAME      L"\\Device\\cloudphone"
#define LINK_NAME     L"\\DosDevices\\cloudphone"

VOID OnUnload(IN PDRIVER_OBJECT DriverObject)
{
	IoDeleteDevice(DriverObject->DeviceObject);
	IoDeleteSymbolicLink(&DeviceLink);
}


extern "C" DRIVER_INITIALIZE DriverEntry;

extern "C" NTSTATUS DriverEntry(IN PDRIVER_OBJECT DriverObject, IN PUNICODE_STRING RegistryPath)
{
	NTSTATUS ntStatus;

	UNREFERENCED_PARAMETER(RegistryPath);
	RtlInitUnicodeString(&DeviceLink, LINK_NAME);
	RtlInitUnicodeString(&DeviceName, DEVICE_NAME);

	ntStatus = IoCreateDevice(
		DriverObject,
		0,
		&DeviceName,
		FILE_DEVICE_UNKNOWN,
		FILE_DEVICE_SECURE_OPEN,
		FALSE,
		&DriverObject->DeviceObject);

	if (!NT_SUCCESS(ntStatus))
	{
		return ntStatus;
	}

	ntStatus = IoCreateSymbolicLink(&DeviceLink, &DeviceName);
	ntStatus = KsInitializeDriver(DriverObject, RegistryPath, &CaptureDeviceDescriptor);
	DriverObject->DriverUnload = OnUnload;

	fpClassCreatefunction = DriverObject->MajorFunction[IRP_MJ_CREATE];
	DriverObject->MajorFunction[IRP_MJ_CREATE] = CCaptureDevice::MyCamCreate;

	fpClassDispatchfunction = DriverObject->MajorFunction[IRP_MJ_DEVICE_CONTROL];
	DriverObject->MajorFunction[IRP_MJ_DEVICE_CONTROL] = CCaptureDevice::MyCamDeviceControl;

	return ntStatus;
}

extern "C" NTSTATUS CCaptureDevice::MyCamCreate(PDEVICE_OBJECT DeviceObject, PIRP Irp)
{
	PIO_STACK_LOCATION irpStack = IoGetCurrentIrpStackLocation(Irp);

	if (irpStack->Parameters.Create.FileAttributes == FILE_ATTRIBUTE_OFFLINE){

		UNREFERENCED_PARAMETER(DeviceObject);
		PAGED_CODE();
		Irp->IoStatus.Status = STATUS_SUCCESS;
		Irp->IoStatus.Information = 0;
		IoCompleteRequest(Irp, IO_NO_INCREMENT);
		return STATUS_SUCCESS;
	}
	return fpClassCreatefunction(DeviceObject, Irp);

}

extern "C" NTSTATUS CCaptureDevice::MyCamDeviceControl(PDEVICE_OBJECT DeviceObject, PIRP Irp)
{
	NTSTATUS ntStatus = STATUS_SUCCESS;
	PIO_STACK_LOCATION	irpStack = IoGetCurrentIrpStackLocation(Irp);
	ULONG				ioControlCode;
//	UCHAR *inBuf;
//	ULONG inBufLength = 0;
	ioControlCode = irpStack->Parameters.DeviceIoControl.IoControlCode;

	switch (irpStack->MajorFunction)
	{
	case IRP_MJ_CREATE:
		DbgPrint("Create \n");
		break;

	case IRP_MJ_CLOSE:
		DbgPrint("Close \n");
		break;

	case IRP_MJ_CLEANUP:
		DbgPrint("Cleanup \n");
		break;
	case IRP_MJ_DEVICE_CONTROL:
		switch (ioControlCode)
		{
		case IOCTL_IMAGE:
/*			DbgPrint("IOCTL_IMAGE case\n");
			inBufLength = irpStack->Parameters.DeviceIoControl.InputBufferLength;
			inBuf = (UCHAR*)Irp->AssociatedIrp.SystemBuffer;
			
			if (inBufLength != 0) {
				//RtlCopyBytes(psyImageBuf_, inBuf, inBufLength > sizeof(psyImageBuf_) ? sizeof(psyImageBuf_) : inBufLength);
			}
			else {
				DbgPrint("[!] IOCTL : IOCTL_TEST - inBufLength Fail\n");
			}
			ntStatus = STATUS_SUCCESS;
			break;*/
		default:
			ntStatus = STATUS_NOT_SUPPORTED;
			break;
		}
		break;
	default:
		ntStatus = STATUS_NOT_SUPPORTED;
		break;

	}

	// Complete Irp
	if (ntStatus == STATUS_SUCCESS)
	{
		// Real return status set in Irp->IoStatus.Status
		Irp->IoStatus.Status = ntStatus;
		IoCompleteRequest(Irp, IO_NO_INCREMENT);
		ntStatus = STATUS_SUCCESS;
	}
	else
	{
		ntStatus = fpClassDispatchfunction(DeviceObject, Irp);
	}
	return ntStatus;
}


