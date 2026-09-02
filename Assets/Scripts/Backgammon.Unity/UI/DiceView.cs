using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Backgammon.Unity
{
    /// <summary>
    /// Shows the two dice as pip-face sprites and plays a brief shuffle animation when told a
    /// roll happened, landing on the real result. Purely presentational — GameController is the
    /// only thing that calls PlayRoll, and only right after a roll actually occurs.
    /// </summary>
    public class DiceView : MonoBehaviour
    {
        [SerializeField] private Image die1Image;
        [SerializeField] private Image die2Image;
        [Tooltip("Six pip-face sprites, in order: index 0 = the face showing 1 pip, ... index 5 = the face showing 6 pips.")]
        [SerializeField] private Sprite[] pipSprites;
        [SerializeField] private float rollDuration = 0.5f;
        [SerializeField] private float rollFrameInterval = 0.06f;

        private Coroutine _activeRoll;
        private readonly System.Random _shuffleRandom = new System.Random();

        private void Awake()
        {
            die1Image.enabled = false;
            die2Image.enabled = false;
        }

        /// <summary>Plays the shuffle animation, ending on the given die values (1-6 each).</summary>
        public void PlayRoll(int finalDie1, int finalDie2)
        {
            if (_activeRoll != null)
            {
                StopCoroutine(_activeRoll);
            }
            _activeRoll = StartCoroutine(RollCoroutine(finalDie1, finalDie2));
        }

        private IEnumerator RollCoroutine(int finalDie1, int finalDie2)
        {
            die1Image.enabled = true;
            die2Image.enabled = true;

            float elapsed = 0f;
            while (elapsed < rollDuration)
            {
                die1Image.sprite = pipSprites[_shuffleRandom.Next(0, pipSprites.Length)];
                die2Image.sprite = pipSprites[_shuffleRandom.Next(0, pipSprites.Length)];
                yield return new WaitForSeconds(rollFrameInterval);
                elapsed += rollFrameInterval;
            }

            die1Image.sprite = pipSprites[finalDie1 - 1];
            die2Image.sprite = pipSprites[finalDie2 - 1];
            _activeRoll = null;
        }
    }
}
