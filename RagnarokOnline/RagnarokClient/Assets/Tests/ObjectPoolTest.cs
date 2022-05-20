using NUnit.Framework;
using RO;
using RO.Media;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class ObjectPoolTest
    {
        GameObject _objPool;
        // A Test behaves as an ordinary method
        [Test]
        public void ObjectPoolInit()
        {
            Assert.IsNull(new ObjectPoll(), "Object pool can be instantiated without game object");
            _objPool = new GameObject("ObjectPool", new System.Type[] { typeof(ObjectPoll) });
            Assert.IsNotNull(_objPool, "Object pool game object was not instantiated");
            Assert.IsNotNull(_objPool.GetComponent<ObjectPoll>(), "Object pool gameObj does not contain static object pool class");
        }

        [UnityTest]
        public IEnumerator ObjectPoolPlayerAnimator()
        {
            ObjectPoolInit();
            yield return new WaitForEndOfFrame();

            //Get player 1
            Assert.IsNull(GameObject.Find("Player"), "Object poll started with a player created already");
            GameObject playerAnimator = ObjectPoll.PlayerAnimatorControllersPoll;
            Assert.IsNotNull(GameObject.Find("Player"), "Object poll did not create first player on first request");
            Assert.AreEqual(GameObject.Find("Player"), playerAnimator, "Player object created does not match returned player");

            //Get player 2 and confirm is different from player 1
            GameObject playerAnimator2 = ObjectPoll.PlayerAnimatorControllersPoll;
            Assert.IsNotNull(playerAnimator2, "Could not get second player");
            Assert.AreNotEqual(playerAnimator, playerAnimator2, "2 subsequent calls to player animator poll returned same player");

            //Return player 2 and confirm it's state
            Assert.AreNotEqual(playerAnimator2.transform.parent, _objPool.transform, "second player parent was already object poll");
            Assert.IsTrue(playerAnimator2.activeSelf, "Player2 was not active before going into object poll");
            Assert.IsTrue(playerAnimator2.GetComponent<PlayerAnimatorController>().enabled, "Player animator controller was not active before going into object poll");
            ObjectPoll.PlayerAnimatorControllersPoll = playerAnimator2;
            Assert.AreEqual(playerAnimator2.transform.parent, _objPool.transform, "Object pool did not overwrite parent for playe2");
            Assert.IsFalse(playerAnimator2.GetComponent<PlayerAnimatorController>().enabled, "Player animator controller was not deactivated once pushed into object poll");
            Assert.IsFalse(playerAnimator2.activeSelf, "Player2 was not deactivated once pushed into object poll");

            //Get player 3 and confirm it's a reuse of player 2
            GameObject playerAnimator3 = ObjectPoll.PlayerAnimatorControllersPoll;
            Assert.AreEqual(playerAnimator2, playerAnimator3, "Object poll did not reuse previously pushed player animator");
            Assert.IsTrue(playerAnimator3.activeSelf, "Player2 was not active after getting from object poll");
            Assert.IsTrue(playerAnimator3.GetComponent<PlayerAnimatorController>().enabled, "Player animator controller was not active after getting from object poll");
        }

        [UnityTest]
        public IEnumerator ObjectPoolEquipment()
        {
            ObjectPoolInit();
            yield return new WaitForEndOfFrame();

            //Confirm that object poll cannot create parts on it's own
            Assert.IsTrue(_objPool.transform.childCount == 0, "Object pool already has objects stored");
            Assert.IsNull(ObjectPoll.EquipmentAnimatorsPoll, "Object pool was able to return an equipment animator");
            Assert.IsNull(ObjectPoll.ShieldAnimatorsPoll, "Object pool was able to return a shield animator");

            GameObject obj1 = new GameObject();
            GameObject obj2 = new GameObject();

            //Test pushing of objects
            ObjectPoll.EquipmentAnimatorsPoll = obj1;
            Assert.IsTrue(_objPool.transform.childCount == 1, "Equipment poll did not store obj1");
            Assert.IsFalse(obj1.activeSelf, "Equipment poll did not deactivate obj1");
            Assert.AreEqual(obj1.transform.parent, _objPool.transform, "Equipment poll did not set obj1 parent");
            ObjectPoll.EquipmentAnimatorsPoll = obj2;
            Assert.IsTrue(_objPool.transform.childCount == 2, "Equipment poll did not store obj2");
            Assert.IsFalse(obj2.activeSelf, "Equipment poll did not deactivate obj2");
            Assert.AreEqual(obj2.transform.parent, _objPool.transform, "Equipment poll did not set obj2 parent");

            //Test popping of objects
            GameObject obj3, obj4;
            obj3 = ObjectPoll.EquipmentAnimatorsPoll;
            Assert.AreEqual(obj3, obj2, "Equipment poll did not return last pushed object");
            Assert.IsTrue(obj3.activeSelf, "Equipment poll did activate obj3 on return");
            obj4 = ObjectPoll.EquipmentAnimatorsPoll;
            Assert.AreEqual(obj4, obj1, "Equipment poll did not return remaining object");
            Assert.IsTrue(obj4.activeSelf, "Equipment poll did activate obj4 on return");
        }

        [UnityTest]
        public IEnumerator ObjectPoolShield()
        {
            ObjectPoolInit();
            yield return new WaitForEndOfFrame();

            //Confirm that object poll cannot create parts on it's own
            Assert.IsTrue(_objPool.transform.childCount == 0, "Object pool already has objects stored");
            Assert.IsNull(ObjectPoll.ShieldAnimatorsPoll, "Object pool was able to return an Shield animator");
            Assert.IsNull(ObjectPoll.ShieldAnimatorsPoll, "Object pool was able to return a shield animator");

            GameObject obj1 = new GameObject();
            GameObject obj2 = new GameObject();

            //Test pushing of objects
            ObjectPoll.ShieldAnimatorsPoll = obj1;
            Assert.IsTrue(_objPool.transform.childCount == 1, "Shield poll did not store obj1");
            Assert.IsFalse(obj1.activeSelf, "Shield poll did not deactivate obj1");
            Assert.AreEqual(obj1.transform.parent, _objPool.transform, "Shield poll did not set obj1 parent");
            ObjectPoll.ShieldAnimatorsPoll = obj2;
            Assert.IsTrue(_objPool.transform.childCount == 2, "Shield poll did not store obj2");
            Assert.IsFalse(obj2.activeSelf, "Shield poll did not deactivate obj2");
            Assert.AreEqual(obj2.transform.parent, _objPool.transform, "Shield poll did not set obj2 parent");

            //Test popping of objects
            GameObject obj3, obj4;
            obj3 = ObjectPoll.ShieldAnimatorsPoll;
            Assert.AreEqual(obj3, obj2, "Shield poll did not return last pushed object");
            Assert.IsTrue(obj3.activeSelf, "Shield poll did activate obj3 on return");
            obj4 = ObjectPoll.ShieldAnimatorsPoll;
            Assert.AreEqual(obj4, obj1, "Shield poll did not return remaining object");
            Assert.IsTrue(obj4.activeSelf, "Shield poll did activate obj4 on return");
        }
    }
}
