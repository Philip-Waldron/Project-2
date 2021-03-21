using System;
using DG.Tweening;
using Project2.Scripts.XR_Player.Common.XR_Input;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Project2.Scripts.Game_Logic
{
    public class GameController : XRInputAbstraction
    {
        [Header("Core References")]
        [SerializeField] private Bomb bomb;
        [SerializeField] private Objective objective;
        [SerializeField] public TextMeshPro bombText;
        [SerializeField] public MeshRenderer healthVisual;
        [Header("Additional References")]
        [SerializeField] public TextMeshPro introText;
        [SerializeField] public TextMeshPro introTimer;
        [SerializeField] private Transform leftDoor, rightDoor;
        [Header("Settings")]
        [SerializeField] private float startTime;
        [SerializeField] private float startHealth;
        [SerializeField, Range(0f, 1f)] private float timeThreshold, healthThreshold, distanceThreshold;
        [Header("Game Events")]
        public UnityEvent onWin;
        public UnityEvent onLose;
        public UnityEvent onStart;
        public UnityEvent onHealthBelowThreshold;
        public UnityEvent onTimeBelowThreshold;
        public UnityEvent onDistanceBelowThreshold;
        public UnityEvent onEjection;

        private bool time, health, distance;
        private bool countdown, finished, coupled;
        private float currentTime, currentHealth, startDistance;
        private static readonly int Health = Shader.PropertyToID("_Health");

        public bool Ejected { get; private set; }

        private float DistanceValue => CurrentDistance / startDistance;
        private float TimeValue => currentTime / startTime;
        private float HealthValue => currentHealth / startHealth;
        private float CurrentDistance => Vector3.Distance(objective.transform.position, bomb.transform.position);

        private static Vector3 HeadPosition => XRInputController.Instance.Position(XRInputController.Check.Head);
        private static Vector3 BombPosition => new Vector3(HeadPosition.x, HeadPosition.y - .65f, HeadPosition.z);

        public static GameController Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            startDistance = CurrentDistance;
            currentHealth = startHealth;
            currentTime = startTime;
            countdown = true;
            objective.metObjective.AddListener(MetObjective);

            DisplayHealth();
            DisplayTime(currentTime);
        }

        public void Update()
        {
            CheckCoupleDecoupleState();
            if (finished) return;
            CheckThresholdStates();
            CountdownTimer();
        }

        private void CheckCoupleDecoupleState()
        {
            // Once the bomb has been initially coupled to the player
            if (coupled)
            {
                // Set it in the right position on the player
                bomb.transform.position = BombPosition;
                bomb.transform.eulerAngles = XRInputController.Instance.NormalisedRotation(XRInputController.Check.Head);
                // Check if you need to eject it from the player
                if (DecoupleTrigger(out XRInputController.Check check))
                {
                    Ejected = true;
                    coupled = false;
                    bomb.Decouple(XRInputController.Instance.Forward(check) * 7.5f);
                    onEjection.Invoke();
                }
            }
            else if (!Ejected && CoupleTrigger)
            {
                bomb.Couple(this);
                bomb.transform.DOMove(BombPosition, .25f).OnComplete(()=> coupled = true);
                bomb.transform.DORotate(XRInputController.Instance.NormalisedRotation(XRInputController.Check.Head), .25f).OnComplete(()=> bomb.transform.SetParent(transform));
                CoupleTrigger = false;

                leftDoor.DOLocalMove(new Vector3(0f, 0f, 2f), 3f);
                rightDoor.DOLocalMove(new Vector3(0f, 0f, -2f), 3f);
                
                onStart.Invoke();
            }
        }

        private void CheckThresholdStates()
        {
            if (!countdown || currentHealth <= 0f)
            {
                FailedObjective();
            }
            if (!time && TimeValue <= timeThreshold)
            {
                TimeThreshold();
            }
            if (!health && HealthValue <= healthThreshold)
            {
                HealthThreshold();
            }
            if (!distance && DistanceValue <= distanceThreshold)
            {
                DistanceThreshold();
            }
        }

        private void CountdownTimer()
        {
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
            introTimer.SetText($"{minutes:00}:{seconds:00}");
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

        private void TimeThreshold()
        {
            time = true;
            onTimeBelowThreshold.Invoke();
        }

        private void HealthThreshold()
        {
            health = true;
            onHealthBelowThreshold.Invoke();
        }

        private void DistanceThreshold()
        {
            distance = true;
            onDistanceBelowThreshold.Invoke();
        }

        private void MetObjective()
        {
            if (finished) return;
            finished = true;
            bombText.SetText("Nice!");
            onWin.Invoke();
        }

        private void FailedObjective()
        {
            if (finished) return;
            finished = true;
            bombText.color = Color.red;
            bombText.SetText("Oof!");
            onLose.Invoke();
        }

        private static bool DecoupleTrigger(out XRInputController.Check check)
        {
            bool state = XRInputController.Instance.InputEvent(XRInputController.XRControllerButton.Primary).State(XRInputController.InputEvents.InputEvent.Transition.Down, out XRInputController.Check setCheck);
            check = setCheck;
            return state;
        }

        public bool CoupleTrigger { private get; set; }
    }
}
