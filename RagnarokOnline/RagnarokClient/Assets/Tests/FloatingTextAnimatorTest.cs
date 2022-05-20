using RO.Common;
using RO.Media;
using UnityEngine;

namespace Tests
{
    public class FloatingTextAnimatorTest : MonoBehaviour
    {
        public enum FloatingTextType : int
        {
            //Number types with animation, sorted by act file animation order
            Miss = 0,
            DamageCrit,
            Lucky,

            DamageWhite,
            DamageStack,
            Heal,
            None
        }

        public uint number = 0;
        public FloatingTextColor floatingTextcolor = FloatingTextColor.White;
        public FloatingTextType floatingTextType = FloatingTextType.None;
        public Vector3 position = Vector3.zero;

        FloatingTextController.Updater _numberUpdater = new FloatingTextController.Updater();

        public void Start()
        {
            Globals.Time = Time.time;
            Globals.TimeSinceLevelLoad = Time.timeSinceLevelLoad;
        }

        public void Update()
        {
            Globals.Time += Time.deltaTime;
            Globals.TimeSinceLevelLoad = Time.timeSinceLevelLoad;

            _numberUpdater.UpdateFloatingTexts();

            if (number == 0)
                return;

            switch (floatingTextType)
            {
                case FloatingTextType.DamageCrit: FloatingTextController.PlayCritDamage(transform, number); break;
                case FloatingTextType.DamageStack: FloatingTextController.PlayStackingNumber(transform, number, 2); break;
                case FloatingTextType.DamageWhite: FloatingTextController.PlayRegularDamage(transform, number, floatingTextcolor); break;
                case FloatingTextType.Heal: FloatingTextController.PlayHeal(transform, number); break;
                case FloatingTextType.Lucky: FloatingTextController.PlayLucky(transform); break;
                case FloatingTextType.Miss: FloatingTextController.PlayMiss(transform, floatingTextcolor); break;
            }
            _numberUpdater.UpdateFloatingTexts();
            number = 0;
        }
    }
}
