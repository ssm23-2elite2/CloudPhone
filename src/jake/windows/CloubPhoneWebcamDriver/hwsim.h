
//
// SCATTER_GATHER_MAPPINGS_MAX:
//
// The maximum number of entries in the hardware's scatter/gather list.  I
// am making this so large for a few reasons:
//
//     1) we're faking this with uncompressed surfaces -- 
//            these are large buffers which will map to a lot of s/g entries
//     2) the fake hardware implementation requires at least one frame's
//            worth of s/g entries to generate a frame
//
#define SCATTER_GATHER_MAPPINGS_MAX 128

//
// SCATTER_GATHER_ENTRY:
//
// This structure is used to keep the scatter gather table for the fake
// hardware as a doubly linked list.
//
typedef struct _SCATTER_GATHER_ENTRY {

	LIST_ENTRY ListEntry;
	PUCHAR Virtual;
	ULONG ByteCount;

} SCATTER_GATHER_ENTRY, *PSCATTER_GATHER_ENTRY;

//
// CHardwareSimulation:
//
// The hardware simulation class.
//
class CHardwareSimulation {

private:

	//
	// The image synthesizer.  This is a piece of code which actually draws
	// the requested images.
	//
	CImageSynthesizer *m_ImageSynth;

	//
	// The synthesis buffer.  This is a private buffer we use to generate the
	// capture image in.  The fake "scatter / gather" mappings are filled
	// in from this buffer during each interrupt.
	//
	PUCHAR m_SynthesisBuffer;

	//
	// Key information regarding the frames we generate.
	//
	LONGLONG m_TimePerFrame;
	ULONG m_Width;
	ULONG m_Height;
	ULONG m_ImageSize;

	//
	// Scatter gather mappings for the simulated hardware.
	//
	KSPIN_LOCK m_ListLock;
	LIST_ENTRY m_ScatterGatherMappings;

	//
	// Lookaside for memory for the scatter / gather entries on the scatter /
	// gather list.
	//
	NPAGED_LOOKASIDE_LIST m_ScatterGatherLookaside;

	//
	// The current state of the fake hardware.
	//
	HARDWARE_STATE m_HardwareState;

	//
	// The pause / stop hardware flag and event.
	//
	BOOLEAN m_StopHardware;
	KEVENT m_HardwareEvent;

	//
	// Maximum number of scatter / gather mappins in the s/g table of the
	// fake hardware.
	//
	ULONG m_ScatterGatherMappingsMax;

	//
	// Number of scatter / gather mappings that have been completed (total)
	// since the start of the hardware or any reset.
	//
	ULONG m_NumMappingsCompleted;

	//
	// Number of scatter / gather mappings that are queued for this hardware.
	//
	ULONG m_ScatterGatherMappingsQueued;
	ULONG m_ScatterGatherBytesQueued;

	//
	// Number of frames skipped due to lack of scatter / gather mappings.
	//
	ULONG m_NumFramesSkipped;

	//
	// The "Interrupt Time".  Number of "fake" interrupts that have occurred
	// since the hardware was started.
	// 
	ULONG m_InterruptTime;

	//
	// The system time at start.
	//
	LARGE_INTEGER m_StartTime;

	//
	// The DPC used to "fake" ISR
	//
	KDPC m_IsrFakeDpc;
	KTIMER m_IsrTimer;

	//
	// The hardware sink that will be used for interrupt notifications.
	//
	IHardwareSink *m_HardwareSink;

	// This is called by the hardware simulation to fill a series of scatter /
	// gather buffers with synthesized data.
	//
	NTSTATUS FillScatterGatherBuffers();

public:

	// The hardware simulation constructor.  Since the new operator will
	// have zeroed the memory, only initialize non-NULL, non-0 fields. 
	//
	CHardwareSimulation(IN IHardwareSink *HardwareSink);

	// The hardware simulation destructor.
	//
	~CHardwareSimulation() {}

	// This is the free callback for the bagged hardware sim.  Not providing
	// one will call ExFreePool, which is not what we want for a constructed
	// C++ object.  This simply deletes the simulation.
	//
	static void Cleanup(IN CHardwareSimulation *HwSim)
	{
		delete HwSim;
	}

	// Called from the simulated interrupt.  First we fake the hardware's
	// actions (at DPC) then we call the "Interrupt service routine" on
	// the hardware sink.
	//
	void FakeHardware();

	// "Start" the fake hardware.  This will start issuing interrupts and 
	// DPC's. 
	//
	// The frame rate, image size, and a synthesizer must be provided.
	//
	NTSTATUS Start(CImageSynthesizer *ImageSynth, IN LONGLONG TimePerFrame, IN ULONG Width, IN ULONG Height, IN ULONG ImageSize);

	// "Pause" or "unpause" the fake hardware.  This will stop issuing 
	// interrupts or DPC's on a pause and restart them on an unpause.  Note
	// that this will not reset counters as a Stop() would.
	//
	NTSTATUS Pause(IN BOOLEAN Pausing);

	// "Stop" the fake hardware.  This will stop issuing interrupts and
	// DPC's.
	//
	NTSTATUS Stop();

	// Program a series of scatter gather mappings into the fake hardware.
	//
	ULONG ProgramScatterGatherMappings(IN PUCHAR *Buffer, IN PKSMAPPING Mappings, IN ULONG MappingsCount, IN ULONG MappingStride);

	// Initialize a piece of simulated hardware.
	//
	static CHardwareSimulation * Initialize(IN KSOBJECT_BAG Bag, IN IHardwareSink *HardwareSink);

	// Read the number of mappings completed since the last hardware reset.
	//
	ULONG ReadNumberOfMappingsCompleted();

};


