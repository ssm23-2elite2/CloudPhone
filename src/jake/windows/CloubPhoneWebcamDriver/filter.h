
class CCaptureFilter {

private:

	//
	// The AVStream filter object associated with this CCaptureFilter.
	//
	PKSFILTER m_Filter;

	// This is the bag cleanup callback for the CCaptureFilter.  Not providing
	// one would cause ExFreePool to be used.  This is not good for C++
	// constructed objects.  We simply delete the object here.
	//
	static void Cleanup(IN CCaptureFilter *CapFilter)
	{
		delete CapFilter;
	}

public:

	// The capture filter object constructor.  Since the new operator will
	// have zeroed the memory, do not bother initializing any NULL or 0
	// fields.  Only initialize non-NULL, non-0 fields.
	//
	CCaptureFilter(IN PKSFILTER Filter) : m_Filter(Filter)
	{
	}

	// The capture filter destructor.
	//
	~CCaptureFilter()
	{
	}

	// This is the filter creation dispatch for the capture filter.  It
	// creates the CCaptureFilter object, associates it with the AVStream
	// object, and bags it for easy cleanup later.
	//
	static NTSTATUS DispatchCreate(IN PKSFILTER Filter, IN PIRP Irp);

};



