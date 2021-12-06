package crc6430282eda421b975b;


public class DataRecorderService_OnFailureListener
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		com.google.android.gms.tasks.OnFailureListener
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onFailure:(Ljava/lang/Exception;)V:GetOnFailure_Ljava_lang_Exception_Handler:Android.Gms.Tasks.IOnFailureListenerInvoker, Xamarin.GooglePlayServices.Tasks\n" +
			"";
		mono.android.Runtime.register ("DataCollection.Droid.Services.DataRecorderService+OnFailureListener, DataCollection.Android", DataRecorderService_OnFailureListener.class, __md_methods);
	}


	public DataRecorderService_OnFailureListener ()
	{
		super ();
		if (getClass () == DataRecorderService_OnFailureListener.class)
			mono.android.TypeManager.Activate ("DataCollection.Droid.Services.DataRecorderService+OnFailureListener, DataCollection.Android", "", this, new java.lang.Object[] {  });
	}


	public void onFailure (java.lang.Exception p0)
	{
		n_onFailure (p0);
	}

	private native void n_onFailure (java.lang.Exception p0);

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
