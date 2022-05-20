using RO.Common;
using RO.Containers;
using UnityEngine;

namespace RO.Media
{
    public class FloatingTextAnimator : MonoBehaviour
    {
        private enum FloatingTextType : int
        {
            //Number types with animation, sorted by act file animation order
            Miss = 0,
            Guard, // this is here for act file offset but it is not used
            DamageCrit,
            Lucky,

            NoActAnimation,
            DamageWhite = NoActAnimation,
            DamageStack, // the yellow one
            Heal,
            None
        }

        //this way we do not need to pass by param or expose floating text type enum
        private static class BundleData
        {
            public readonly static Act Act = null;
            public readonly static Sprite[] NumberSprites = null;
            public readonly static Material[] AnimationMaterials = null;

            static BundleData()
            {
                Act = (Act)AssetBundleProvider.LoadMiscBundleAsset<ScriptableObject>(ConstStrings.FLOATING_TEXT_ACT_NAME);
                AnimationMaterials = new Material[(int)FloatingTextType.None];
                NumberSprites = new Sprite[10]; // there's always 10 digits

                // load all number sprites
                for (int i = 0; i < 10; i++)
                    NumberSprites[i] = AssetBundleProvider.LoadMiscBundleAsset<Sprite>("number_" + i.ToString());

                //Load all animation materials
                AnimationMaterials[(int)FloatingTextType.DamageCrit] = AssetBundleProvider.LoadMiscBundleAsset<Material>("critical_damage");
                AnimationMaterials[(int)FloatingTextType.DamageWhite] = AssetBundleProvider.LoadMiscBundleAsset<Material>("floating_damage");
                AnimationMaterials[(int)FloatingTextType.DamageStack] = AssetBundleProvider.LoadMiscBundleAsset<Material>("stacking_damage");
                AnimationMaterials[(int)FloatingTextType.Miss] = AssetBundleProvider.LoadMiscBundleAsset<Material>("miss_text");
                AnimationMaterials[(int)FloatingTextType.Heal] = AssetBundleProvider.LoadMiscBundleAsset<Material>("heal_number");
                AnimationMaterials[(int)FloatingTextType.Lucky] = AssetBundleProvider.LoadMiscBundleAsset<Material>("act_text");
            }
        }

        //Background can need 2 renderers for lucky animation. We can reuse the second one as a number renderer
        [SerializeField] private SpriteRenderer[] _background = null;
        [SerializeField] private SpriteRenderer[] _numberRenderers = null;

        private const int MAX_DIGITS = 6;
        private const float DIGIT_SPACING = 1.75f;

        private readonly static float[] START_OFFSETS = new float[MAX_DIGITS] { 0.2f, 1.15f, 1.80f, 2.8f, 3.25f, 4.5f };

        //Values gotten directly from msg_act.asset file. These were parsed from RO file so they should not change
        private readonly static int[] MAX_FRAMES_PER_ANIM = new int[(int)FloatingTextType.NoActAnimation] { 9, 22, 15, 28 };

        private static readonly Vector3 MISS_OFFSET = new Vector3(0, 10, 0);
        private static readonly Vector3 LUCKY_OFFSET = new Vector3(-1.5f, 10, 0);
        private static readonly Vector3 HEAL_OFFSET = new Vector3(0, 10, 0);
        private static readonly Vector3 DMG_STACK_OFFSET = new Vector3(0, 10, 0);
        private static readonly Vector3 DMG_CRIT_OFFSET = new Vector3(-5, 13, 0);
        private static readonly Vector3 DMG_FLOAT_OFFSET = new Vector3(0, 10, 0);

        private float _nextUpdate = 0;
        private int _currentFrame = 0;
        private Vector4 _startTime = Vector4.zero;
        private Vector3 _scale = Vector3.one;
        private Color _color = Color.white;

        private MaterialPropertyBlock _propertyBlock;
        private FloatingTextType _floatingTextAnimationType;

        public void Awake()
        {
            // this is to make sure static data was loaded before we use it
            // hopefully this will optimize it's loading place
            var init = BundleData.NumberSprites.Length;

            _propertyBlock = new MaterialPropertyBlock();
        }

        public void UpdateAnimation()
        {
            //Don't do anything for following conditions
            if (_floatingTextAnimationType >= FloatingTextType.NoActAnimation //animations that don't require act
                || _currentFrame == MAX_FRAMES_PER_ANIM[(int)_floatingTextAnimationType] // if animation has reached the end. These animations dont loop
                || Globals.Time < _nextUpdate) // If not enough time has passed for next update
                return;


            _nextUpdate = _nextUpdate + BundleData.Act.Actions[(int)_floatingTextAnimationType].delay * MediaConstants.ACTION_DELAY_BASE_TIME;

            //if animation is lagging behind time we should skip a frame
            if (_nextUpdate < Globals.Time && _currentFrame + 1 != MAX_FRAMES_PER_ANIM[(int)_floatingTextAnimationType])
            {
                _currentFrame++;
                UpdateAnimation();
                return; // dont update, we'll update renderer on recursive one
            }

            //Any animation with an act animation must have a background
            var frameData = BundleData.Act.Actions[(int)_floatingTextAnimationType].Frames[_currentFrame].frameData;


            //Assign the background frame animation. 
            _background[0].GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetVector(MediaConstants.SHADER_POSITION_PROPERTY_ID, new Vector2(frameData.PositionOffset[0].x, -frameData.PositionOffset[0].y));
            _propertyBlock.SetVector(MediaConstants.SHADER_SCALE_PROPERTY_ID, new Vector3(frameData.Scale[0].x, frameData.Scale[0].y, 1));
            _background[0].SetPropertyBlock(_propertyBlock);

            _background[0].color = frameData.Color[0];
            _background[0].sprite = BundleData.Act.Sprites[frameData.SpriteId[0]];

            //Lucky animation has more than 1 sprite. Should be the only one. Change this if to something else if this changes
            if (_floatingTextAnimationType == FloatingTextType.Lucky)
            {
                //But not all frames uses both sprites...
                if (frameData.SpriteId.Length == 2)
                {
                    _background[1].enabled = true;

                    _background[1].GetPropertyBlock(_propertyBlock);
                    _propertyBlock.SetVector(MediaConstants.SHADER_POSITION_PROPERTY_ID, new Vector2(frameData.PositionOffset[1].x, -frameData.PositionOffset[1].y));
                    _propertyBlock.SetVector(MediaConstants.SHADER_SCALE_PROPERTY_ID, new Vector3(frameData.Scale[1].x, frameData.Scale[1].y, 1));
                    _background[1].SetPropertyBlock(_propertyBlock);

                    _background[1].color = frameData.Color[1];
                    _background[1].sprite = BundleData.Act.Sprites[frameData.SpriteId[1]];
                }
                else
                    _background[1].enabled = false;
            }

            _currentFrame++;
        }

        public void AnimateLucky(short sortingOrder)
        {
            _nextUpdate = Globals.Time;
            _currentFrame = 0;
            _floatingTextAnimationType = FloatingTextType.Lucky;

            transform.localPosition = LUCKY_OFFSET;

            //Lucky has 2 sprites
            _background[0].enabled = true;
            _background[1].enabled = true;

            _background[0].sortingOrder = sortingOrder;
            _background[1].sortingOrder = sortingOrder + 1;

            _background[0].sharedMaterial = BundleData.AnimationMaterials[(int)FloatingTextType.Lucky];
            _background[1].sharedMaterial = BundleData.AnimationMaterials[(int)FloatingTextType.Lucky];

            for (int i = 0; i < MAX_DIGITS; i++)
                _numberRenderers[i].enabled = false;
        }

        public void AnimateMiss(FloatingTextColor color, short sortingOrder)
        {
            //So we ignore act animation for miss
            _floatingTextAnimationType = FloatingTextType.NoActAnimation;

            transform.localPosition = MISS_OFFSET;

            _background[0].enabled = true;
            _background[0].sharedMaterial = BundleData.AnimationMaterials[(int)FloatingTextType.Miss];

            if (color == FloatingTextColor.Red)
                _background[0].color = Color.red;
            else
                _background[0].color = Color.white;

            //Set start time
            _background[0].GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetVector(MediaConstants.SHADER_START_TIME_ID, new Vector2(Globals.TimeSinceLevelLoad, 0));
            _background[0].SetPropertyBlock(_propertyBlock);

            _background[0].sprite = BundleData.Act.Sprites[BundleData.Act.Actions[(int)FloatingTextType.Miss].Frames[0].frameData.SpriteId[0]];
            _background[0].sortingOrder = sortingOrder;

            //De-activate other renderers
            for (int i = 0; i < MAX_DIGITS; i++)
                _numberRenderers[i].enabled = false;
        }

        public void AnimateFloatingNumber(uint number, FloatingTextColor color, short sortingOrder)
        {
            int nDigits = ParseAndSetDigitSprites(number, (short)(sortingOrder + 1));

            //So we ignore act animation for miss
            _floatingTextAnimationType = FloatingTextType.NoActAnimation;

            transform.localPosition = DMG_FLOAT_OFFSET;

            //Time to pass into the shader
            _startTime.x = Globals.TimeSinceLevelLoad;

            //Normal floating has no background
            _background[0].enabled = false;

            _color = color == FloatingTextColor.Red ? Color.red : Color.white;

            SetDigitRenderersMaterial(nDigits, BundleData.AnimationMaterials[(int)FloatingTextType.DamageWhite]);
        }

        public void AnimateCriticalNumber(uint number, short sortingOrder)
        {
            int nDigits = ParseAndSetDigitSprites(number, (short)(sortingOrder + 1));

            _nextUpdate = Globals.Time;
            _currentFrame = 0;
            _floatingTextAnimationType = FloatingTextType.DamageCrit;

            transform.localPosition = DMG_CRIT_OFFSET;

            //Data to pass into the numbers shader
            const float SCALE_DURATION = 0.350f; //200ms
            _startTime.x = Globals.TimeSinceLevelLoad + SCALE_DURATION;

            _color = Color.white;

            //Set the material with critical dmg shader
            _background[0].enabled = true;
            _background[0].sharedMaterial = BundleData.AnimationMaterials[(int)FloatingTextType.DamageCrit];
            //set the background property block
            _background[0].GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetVector(MediaConstants.SHADER_START_TIME_ID, _startTime);
            _propertyBlock.SetVector(MediaConstants.SHADER_POSITION_PROPERTY_ID, Vector3.zero);
            _background[0].SetPropertyBlock(_propertyBlock);

            _background[0].sortingOrder = sortingOrder;

            SetDigitRenderersMaterial(nDigits, BundleData.AnimationMaterials[(int)FloatingTextType.DamageCrit]);
        }

        public void AnimateStackingNumber(uint number, short sortingOrder)
        {
            int nDigits = ParseAndSetDigitSprites(number, (short)(sortingOrder + 1));

            transform.localPosition = DMG_STACK_OFFSET;

            //Disable act animation
            _floatingTextAnimationType = FloatingTextType.NoActAnimation;

            //Data to pass into the numbers shader
            _startTime.x = Globals.TimeSinceLevelLoad;
            _startTime.y = _startTime.x + 2;

            _color = Color.white;

            //Stacking dmg has no background
            _background[0].enabled = false;

            SetDigitRenderersMaterial(nDigits, BundleData.AnimationMaterials[(int)FloatingTextType.DamageStack]);
        }

        public void AnimateHeal(uint number, short sortingOrder)
        {
            int nDigits = ParseAndSetDigitSprites(number, (short)(sortingOrder + 1));

            transform.localPosition = HEAL_OFFSET;

            //disable act animation
            _floatingTextAnimationType = FloatingTextType.NoActAnimation;

            //Data to pass into the numbers shader
            _startTime.x = Globals.TimeSinceLevelLoad;

            //Heal has no background
            _background[0].enabled = false;

            _color = Color.white;

            SetDigitRenderersMaterial(nDigits, BundleData.AnimationMaterials[(int)FloatingTextType.Heal]);
        }

        private void SetDigitRenderersMaterial(int nDigits, Material material)
        {
            //Set the property block to the digits too
            for (int i = 0; i < nDigits; i++)
            {
                _numberRenderers[i].sharedMaterial = material;

                //reposition digit
                Vector2 digitWorldPosition;
                digitWorldPosition.x = START_OFFSETS[nDigits - 1] - DIGIT_SPACING * i;
                digitWorldPosition.y = 0.5f;

                _numberRenderers[i].GetPropertyBlock(_propertyBlock);
                _propertyBlock.SetVector(MediaConstants.SHADER_START_TIME_ID, _startTime);
                _propertyBlock.SetVector(MediaConstants.SHADER_POSITION_PROPERTY_ID, digitWorldPosition);
                _propertyBlock.SetVector(MediaConstants.SHADER_SCALE_PROPERTY_ID, _scale);
                _numberRenderers[i].SetPropertyBlock(_propertyBlock);

                _numberRenderers[i].color = _color;
            }
        }

        private int ParseAndSetDigitSprites(uint number, short sortingOrder)
        {
            uint[] digits = new uint[MAX_DIGITS];
            int nDigits = 0;
            int rightZeroes = 0;

            //Parse digits from int
            for (int i = 0; i < MAX_DIGITS; i++)
            {
                digits[i] = number % 10;
                number /= 10;

                //We need to count how many digits we have to center them later, not counting left zeroes
                if (digits[i] == 0)
                    rightZeroes++;
                else
                {
                    nDigits += rightZeroes + 1;
                    rightZeroes = 0;
                }
            }

            for (int i = 0; i < MAX_DIGITS; i++)
            {
                //Enable renderers that are within nDigits range
                _numberRenderers[i].enabled = i < nDigits;
                //Only assign sprite and send data if it's enabled
                if (_numberRenderers[i].enabled)
                {
                    _numberRenderers[i].sprite = BundleData.NumberSprites[digits[i]];
                    _numberRenderers[i].sortingOrder = sortingOrder;
                }
            }

            //return how many digits we had
            return nDigits;
        }

        private void OnDisable()
        {
            for (int i = 0; i < MAX_DIGITS; i++)
            {
                _numberRenderers[i].enabled = false;
                _numberRenderers[i].transform.localScale = Vector3.one; //in case scale was modified
            }
            _background[0].enabled = false;
        }
    }
}
