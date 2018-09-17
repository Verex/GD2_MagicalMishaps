using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIControllerGame : MonoBehaviour
{
	[SerializeField] private Text text_Score;
	[SerializeField] private Image img_Health;

	private float maxHealthWidth;
	private float currentHealth = 0;
	private int currentScore = 0;
	public Player playerTarget;

    private IEnumerator UIUpdate()
    {
        while (true)
        {
			if (playerTarget != null)
			{
				currentHealth = playerTarget.health;
				currentScore = playerTarget.score;

				Rect current = img_Health.rectTransform.rect;
				img_Health.rectTransform.sizeDelta = new Vector2(maxHealthWidth * (currentHealth / playerTarget.maxHealth), current.height);

				text_Score.text = currentScore.ToString();

				yield return new WaitUntil(() => currentHealth != playerTarget.health || currentScore != playerTarget.score);
			}

            yield return null;
        }
    }

    // Use this for initialization
    void Start()
    {
		// Get original width.
		maxHealthWidth = img_Health.rectTransform.rect.width;

		// Start update coroutine.
		StartCoroutine(UIUpdate());
    }
}
