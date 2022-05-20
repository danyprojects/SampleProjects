using RO.Common;
using RO.Databases;
using RO.UI;
using UnityEngine;
namespace Tests
{
    public class UIBuffTest : MonoBehaviour
    {
        public BuffPanelController BuffPanelController;

        public BuffIDs AddBuff = BuffIDs.Last;
        public BuffIDs RemoveBuff = BuffIDs.Last;

        public float duration = 30;

        public void Start()
        {
            Globals.Time = Time.time;
            Globals.TimeSinceLevelLoad = Time.timeSinceLevelLoad;

            //Test buffs
            BuffPanelController.AddPermanentBuff(BuffIDs.EnergyCoat); // green
            BuffPanelController.AddPermanentBuff(BuffIDs.Weight50); //red
            /* BuffPanelController.AddBuff(BuffIDs.HellsPower, 30); //purple
             BuffPanelController.AddBuff(BuffIDs.ExpBuff, 30); //yellow
             BuffPanelController.AddBuff(BuffIDs.Kaahi, 15); //DarkBlue
             BuffPanelController.AddBuff(BuffIDs.Endure, 70); //Blue
             BuffPanelController.AddBuff(BuffIDs.AttentionConcentrate, 20);  //green
             BuffPanelController.AddBuff(BuffIDs.Blessing, 85); //white
             BuffPanelController.AddBuff(BuffIDs.SteelBody, 30); //dark yellow*/

            /* for(int i=9;i<20;i++)
               BuffPanelController.AddBuff((BuffIDs)i+2, 30);*/
        }

        public void Update()
        {
            Globals.Time += Time.deltaTime; //This is the clock that will be used everywhere to get current time
                                            // This will tell us if we need to frame skip. Since it's a heavy calculation, do it here so we only calc once per loop
            Globals.TimeSinceLevelLoad = Time.timeSinceLevelLoad;

            if (AddBuff != BuffIDs.Last)
            {
                if (duration == 0)
                    BuffPanelController.AddPermanentBuff(AddBuff);
                else
                    BuffPanelController.AddBuff(AddBuff, duration);

                AddBuff = BuffIDs.Last;
            }
            if (RemoveBuff != BuffIDs.Last)
            {
                BuffPanelController.RemoveBuff(RemoveBuff);
                RemoveBuff = BuffIDs.Last;
            }
        }
    }
}
