using NUnit.Framework;
using RO;
using RO.Common;
using RO.MapObjects;
using RO.Media;
using System.Reflection;
using UnityEngine;

namespace Tests
{
    public class EffectsControllerUnitTests : MonoBehaviour
    {
        EffectsAnimatorController.Updater updater = null;

        private void Init()
        {
            GameObject soundController = new GameObject("SoundController");
            soundController.AddComponent<AudioSource>();

            GameObject objectPoolObj = new GameObject("Object pool");
            var objectPool = objectPoolObj.AddComponent<ObjectPoll>();

            objectPool.GetType().GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(objectPool, null);
            updater = new EffectsAnimatorController.Updater();
        }

        [Test]
        public void TestFuncEffects()
        {
            Init();

            Globals.FrameIncrement = 1;

            GameObject source = new GameObject("Source");
            source.transform.position = Vector3.zero;
            GameObject target = new GameObject("Target");
            target.transform.position = new Vector3(20, 0, 20);

            Monster monster = new Monster(0, RO.Databases.MonsterIDs.AbysmalKnight);

            //Test natural effect end
            string s = "Natural effect end: ";
            monster.effects.Add(EffectsAnimatorController.PlayJupitelThunder(source.transform, monster.transform, monster.RemoveEffect));

            int count = 0;
            while (monster.effects.Count != 0 && count < 1000)
            {
                updater.UpdateEffects();
                count++;
            }
            Assert.IsTrue(count < 1000, s + "Exited by count");
            Assert.IsTrue(monster.effects.Count == 0, s + "Monster still has effect in list");

            //Test cancel by monster disable
            s = "Test by monster disable: ";
            monster.effects.Add(EffectsAnimatorController.PlayJupitelThunder(source.transform, monster.transform, monster.RemoveEffect));
            monster.IsEnabled = false;
            updater.UpdateEffects();
            Assert.IsTrue(monster.effects.Count == 0, s + "Monster still has effect in list");

            //Test cancel before end
            s = "Test cancel before end: ";
            monster.IsEnabled = true;
            monster.effects.Add(EffectsAnimatorController.PlayJupitelThunder(source.transform, monster.transform, monster.RemoveEffect));
            count = 0;
            while (monster.effects.Count != 0 && count < 24)
            {
                updater.UpdateEffects();
                count++;
            }
            monster.IsEnabled = false;
            updater.UpdateEffects();
            Assert.IsTrue(monster.effects.Count == 0, s + "Monster still has effect in list");

            //Test multiple effects
            s = "Test multiple effects: ";
            monster.IsEnabled = true;

            monster.effects.Add(EffectsAnimatorController.PlayJupitelThunder(source.transform, monster.transform, monster.RemoveEffect));
            monster.effects.Add(EffectsAnimatorController.PlayJupitelThunder(source.transform, monster.transform, monster.RemoveEffect));
            monster.effects.Add(EffectsAnimatorController.PlayJupitelThunder(source.transform, monster.transform, monster.RemoveEffect));
            monster.effects.Add(EffectsAnimatorController.PlayJupitelThunder(source.transform, monster.transform, monster.RemoveEffect));
            monster.effects.Add(EffectsAnimatorController.PlayJupitelThunder(source.transform, monster.transform, monster.RemoveEffect));
            monster.effects.Add(EffectsAnimatorController.PlayJupitelThunder(source.transform, monster.transform, monster.RemoveEffect));

            monster.IsEnabled = false;

            updater.UpdateEffects();
            Assert.IsTrue(monster.effects.Count == 0, s + "Monster still has effect in list");
        }
    }
}
