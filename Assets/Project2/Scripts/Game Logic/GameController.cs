using System;
using Project2.Scripts.XR_Player.Common.XR_Input;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Project2.Scripts.Game_Logic
{
    public class GameController : XRInputAbstraction
    {
        [Header("References")]
        [SerializeField] private Bomb bomb;
        [SerializeField] private Objective objective;
        [SerializeField] public TextMeshPro bombText;
        [SerializeField] public MeshRenderer healthVisual;
        [Header("Settings")]
        [SerializeField] private float startTime;
        [SerializeField] private float startHealth;
        [Header("Game Events")] 
        public UnityEvent onWin;
        public UnityEvent onLose;

        private bool countdown, finished, coupled = true;
        private float currentTime, currentHealth;
        private static readonly int Health = Shader.PropertyToID("_Health");

        private float TimeValue => currentTime / startTime;
        private float HealthValue => currentHealth / startHealth;

        private static Vector3 HeadPosition => XRInputController.Position(XRInputController.Check.Head);
        private static Vector3 BombPosition => new Vector3(HeadPosition.x, HeadPosition.y - .65f, HeadPosition.z);

        private void Start()
        {
            currentHealth = startHealth;
            currentTime = startTime;
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
                bomb.transform.eulerAngles = XRInputController.NormalisedRotation(XRInputController.Check.Head);
            }
            if (coupled && XRInputController.InputEvent(XRInputController.XRControllerButton.Primary).State(XRInputController.InputEvents.InputEvent.Transition.Down, out XRInputController.Check check))
            {
                coupled = false;
                bomb.Eject(XRInputController.Velocity(check) * 5f, this);
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
            
            bombText.SetText($"{minutes:00}:{seconds:00}");
        }

        private void DisplayHealth()
        {
            healthVisual.material.SetFloat(Health, HealthValue);
        }
        
        private void OnCollisionEnter(Collision collision)
        {
            if (!coupled) return;
            Collision(collision.relativeVelocity.magnitude);
        }

        public void Collision(float damage)
        {
            currentHealth -= damage;
            currentHealth = Mathf.Clamp(currentHealth, 0f, startHealth);
            DisplayHealth();
        }

        private void MetObjective()
        {
            finished = true;
            bombText.SetText("Nice!");
            onWin.Invoke();
        }
        
        private void FailedObjective()
        {
            finished = true;
            bombText.color = Color.red;
            bombText.SetText("Oof!");
            onLose.Invoke();
        }
    }
}
