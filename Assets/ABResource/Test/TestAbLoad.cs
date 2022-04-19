using UnityEngine;
using Asgard;
using Asgard.Resource;

public class TestAbLoad : MonoBehaviour
{
    private static TestAbLoad minstance = null;

    public static TestAbLoad instance
    {
        get
        {
            if (minstance == null)
                minstance = new TestAbLoad();
            return minstance;
        }
    }


    string testAbName6 = "tanktrack/prefab/kv1.unity3d";

    public void TestGetObj()
    {
        ABResChecker.Instance.GetLatestVersion(() => 
        {
            ABResExplorer.Instance.GetResourceY(testAbName6, OnGetTest, null);
            //BaseResource res = Asgard.AsgardGame.AbResExplorer.GetResourceT(testAbName6);
            //GameObject go = res.InstanceObj;
        });
        
    }


    private void OnGetTest(BaseResource baseResource)
    {
        GameObject go = baseResource.InstanceObj;
        baseResource.UnLoad();
        ABResExplorer.Instance.GetResourceY(testAbName6, OnGetTest2, null);
    }

    private void OnGetTest2(BaseResource baseResource)
    {
        GameObject go = baseResource.InstanceObj;
    }

    void Start()
    {
 
    }

}
