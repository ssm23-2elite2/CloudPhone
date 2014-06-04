#include <initguid.h>

typedef struct _STREAM_POINTER_CONTEXT {

	PUCHAR BufferVirtual;

} STREAM_POINTER_CONTEXT, *PSTREAM_POINTER_CONTEXT;

//
// CCapturePin:
//
// The video capture pin class.
//
class CCapturePin :
	public ICaptureSink {

private:

	PKSPIN m_Pin;
	CCaptureDevice *m_Device;
	HARDWARE_STATE m_HardwareState;
	PIKSREFERENCECLOCK m_Clock;
	PKS_VIDEOINFOHEADER m_VideoInfoHeader;
	PKSSTREAM_POINTER m_PreviousStreamPointer;

	BOOLEAN m_PendIo;
	BOOLEAN m_AcquiredResources;

	LONGLONG m_PresentationTime;
	NTSTATUS CleanupReferences();
	NTSTATUS SetState(IN KSSTATE ToState, IN KSSTATE FromState);
	NTSTATUS Process();
	PKS_VIDEOINFOHEADER CaptureVideoInfoHeader();

	static void Cleanup(IN CCapturePin *Pin)
	{
		delete Pin;
	}

public:

	CCapturePin(IN PKSPIN Pin);
	~CCapturePin() {}

	virtual void CompleteMappings(IN ULONG NumMappings);

	static NTSTATUS DispatchCreate(IN PKSPIN Pin, IN PIRP Irp);
	static NTSTATUS DispatchSetState(IN PKSPIN Pin, IN KSSTATE ToState, IN KSSTATE FromState)
	{
		return (reinterpret_cast <CCapturePin *> (Pin->Context))->SetState(ToState, FromState);
	}

	static NTSTATUS DispatchSetFormat(IN PKSPIN Pin, IN PKSDATAFORMAT OldFormat OPTIONAL, IN PKSMULTIPLE_ITEM OldAttributeList OPTIONAL, IN const KSDATARANGE *DataRange, IN const KSATTRIBUTE_LIST *AttributeRange OPTIONAL);
	static NTSTATUS DispatchProcess(IN PKSPIN Pin)
	{
		return (reinterpret_cast <CCapturePin *> (Pin->Context))->Process();
	}
	static NTSTATUS IntersectHandler(IN PKSFILTER Filter, IN PIRP Irp, IN PKSP_PIN PinInstance, IN PKSDATARANGE CallerDataRange, IN PKSDATARANGE DescriptorDataRange, IN ULONG BufferSize, OUT PVOID Data OPTIONAL, OUT PULONG DataSize);
};
