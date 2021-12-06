package crc6430282eda421b975b;


public class DataRecorderService_NotificationTimer
	extends android.os.CountDownTimer
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onFinish:()V:GetOnFinishHandler\n" +
			"n_onTick:(J)V:GetOnTick_JHandler\n" +
			"";
		mono.android.Runtime.register ("DataCollection.Droid.Services.DataRecorderService+NotificationTimer, DataCollection.Android", DataRecorderService_NotificationTimer.class, __md_methods);
	}


	public DataRecorderService_NotificationTimer (long p0, long p1)
	{
		super (p0, p1);
		if (getClass () == DataRecorderService_NotificationTimer.class)
			mono.android.TypeManager.Activate ("DataCollection.Droid.Services.DataRecorderService+NotificationTimer, DataCollection.Android", "System.Int64, mscorlib:System.Int64, mscorlib", this, new java.lang.Object[] { p0, p1 });
	}


	public void onFinish ()
	{
		n_onFinish ();
	}

	private native void n_onFinish ();


	public void onTick (long p0)
	{
		n_onTick (p0);
	}

	private native void n_onTick (long p0);

	private java.util.ArrayList refList;
	public void monodroidAddReference (java.lang.Object obj)
	{
		if (refList == null)
			refList = new java.util.ArrayList ();
		refList.add (obj);
	}

	public void monodroidClearReferences ()
	{
		if (refList != null)
			refList.clear ();
	}
}
