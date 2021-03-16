using System;
using Project2.Scripts.XR_Player.Common.XR_Input;
using TMPro;
using UnityEngine;

namespace Project2.Scripts.Game_Logic
{
    public class GameController : XRInputAbstraction
    {
        [SerializeField] private Bomb bomb;
        [SerializeField] private Objective objective;
        [SerializeField] private float startDuration, startHealth;
        [SerializeField] public TextMeshPro timeText, healthText;

        private bool countdown, finished, coupled = true;
        private float currentTime, currentHealth;

        private static Vector3 HeadPosition => XRInputController.Position(XRInputController.Check.Head);
        private static Vector3 BombPosition => new Vector3(HeadPosition.x, HeadPosition.y - .65f, HeadPosition.z);

        private void Start()
        {
            currentHealth = startHealth;
            currentTime = startDuration;
            countdown = true;
            objective.metObjective.AddListener(MetObjective);
            
            DisplayHealth();
            DisplayTime(currentTime);
        }

        public void Update()
        {
            if (coupled)
            {
                bomb.transform.position = BombPosition;
            }
            if (coupled && XRInputController.InputEvent(XRInputController.XRControllerButton.Primary).State(XRInputController.InputEvents.InputEvent.Transition.Down, out XRInputController.Check check))
            {
                coupled = false;
                bomb.Eject(XRInputController.Velocity(XRInputController.Check.Head), this);
            }
            
            if (finished) return;
            
            if (!countdown || currentHealth <= 0f)
            {
                FailedObjective();
            }
            
            if (countdown)
            {
                if (currentTime > 0)
                {
                    currentTime -= Time.deltaTime;
                    DisplayTime(currentTime);
                }
                else
                {
                    currentTime = 0;
                    countdown = false;
                }
            }
        }

        private void DisplayTime(float timeToDisplay)
        {
            timeToDisplay += 1;
            
            float minutes = Mathf.FloorToInt(timeToDisplay / 60); 
            float seconds = Mathf.FloorToInt(timeToDisplay % 60);
            
            timeText.SetText($"{minutes:00}:{seconds:00}");
        }

        private void DisplayHealth()
        {
            healthText.SetText($"{currentHealth} / {startHealth}");
        }
        
        private void OnCollisionEnter(Collision collision)
        {
            if (!coupled) return;
            Collision();
        }

        public void Collision()
        {
            currentHealth -= 10f;
            DisplayHealth();
        }

        private void MetObjective()
        {
            finished = true;
            Debug.Log("NICE!");
        }
        
        private void FailedObjective()
        {
            finished = true;
            Debug.Log("OOF");
        }
    }
}
