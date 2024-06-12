using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TOMBSATYR
{
    public struct RunTime
    {
        public string Start;
        public string Destination;
        public float Time;
    }
    
    public class SpeedrunTimer : MonoBehaviour
    {
        public GameObject Times;
        public RunTime LastRun;
        public Checkpoint StartCheckpoint;
        public float Timer;
        public bool bActive = false;
        public List<RunTime> SpeedrunTimes = new List<RunTime>(); // Initialize the list
        private TextMeshProUGUI TMP_CheckpointName;
        private TextMeshProUGUI TMP_SpeedrunTimes;
        private Coroutine TextFade;
        
        // Start is called before the first frame update
        void Start()
        {
            TMP_SpeedrunTimes = Times.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
            TMP_CheckpointName = Times.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>();
        }

        // Update is called once per frame
        void Update()
        {
            if (bActive)
            {
                Timer += Time.deltaTime;
            }
        }

        protected void OnTriggerEnter(Collider other)
        {

            
            if (other.gameObject.TryGetComponent<Checkpoint>(out Checkpoint checkpoint))
            {
                bActive = false;

                if (TextFade != null)
                {
                    StopCoroutine(TextFade);
                    TMP_CheckpointName.gameObject.SetActive(true);
                    Color originalColor = TMP_CheckpointName.color;
                    TMP_CheckpointName.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
                }
                
                Times.SetActive(true);
                
                TMP_CheckpointName.text = checkpoint.Name;
                
                if (StartCheckpoint == null || checkpoint == StartCheckpoint)
                {
                    TMP_SpeedrunTimes.text = GetTimes(checkpoint.Name);
                    TMP_SpeedrunTimes.gameObject.SetActive(true);
                    return;
                }
                
                
                RunTime newTime;
                newTime.Start = StartCheckpoint.Name;
                newTime.Destination = checkpoint.Name;
                newTime.Time = Timer;
                
                bool bTimeFound = false;
                for(int i = 0; i < SpeedrunTimes.Count; i++)
                {
                    if (SpeedrunTimes[i].Start == StartCheckpoint.Name && SpeedrunTimes[i].Destination == checkpoint.Name)
                    {
                        bTimeFound = true;
                        if (SpeedrunTimes[i].Time > Timer) // Update only if the new time is faster
                        {
                            SpeedrunTimes[i] = newTime; // Assign the modified copy back to the list   
                        }
                    }
                }

                if (!bTimeFound)
                {
                    SpeedrunTimes.Add(newTime);
                }

                StartCheckpoint = checkpoint;
                Timer = 0f;
                
                TMP_SpeedrunTimes.text = GetTimes(checkpoint.Name);
                TMP_SpeedrunTimes.gameObject.SetActive(true);
            }
        }

        protected void OnTriggerExit(Collider other)
        {
            if (other.gameObject.TryGetComponent<Checkpoint>(out Checkpoint checkpoint))
            {
                TMP_SpeedrunTimes.gameObject.SetActive(false);

                HandleCoroutine();
                StartCheckpoint = checkpoint;
                bActive = true;
                Timer = 0f;
            }
        }

        private void HandleCoroutine()
        {
            if (TextFade == null)
            {
                TextFade = StartCoroutine(FadeAndActivateText());
            }
            else
            {
                StopCoroutine(FadeAndActivateText());
                TextFade = StartCoroutine(FadeAndActivateText());
            }
            
        }
        
        private IEnumerator FadeAndActivateText()
        {
            // Get the TextMeshPro component from the child
            TextMeshProUGUI textMesh = Times.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            // Ensure the textMesh object is active before starting the fade
            textMesh.gameObject.SetActive(true);

            // Fade out the text
            float elapsedTime = 0f;
            Color originalColor = textMesh.color;

            while (elapsedTime < 2f) //fade duration
            {
                float alpha = Mathf.Lerp(1f, 0f, elapsedTime / 2f); //fade duration
                textMesh.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Ensure the alpha is set to 0
            textMesh.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);

            // Set the text object to active and reset alpha to 1
            textMesh.gameObject.SetActive(true);
            textMesh.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);

            // Disable the full Times object
            Times.gameObject.SetActive(false);
        }
        
        public static string FormatTime(float timeInSeconds)
        {
            int minutes = (int)timeInSeconds / 60;
            int seconds = (int)timeInSeconds % 60;
            int milliseconds = (int)((timeInSeconds - (minutes * 60) - seconds) * 1000);

            return string.Format("{0:D2}m {1:D2}.{2:D2}s", minutes, seconds, milliseconds / 10);
        }

        public string GetTimes(string checkpointName)
        {
            string TimesText = "";
            foreach (RunTime time in SpeedrunTimes)
            {
                if (time.Destination == checkpointName)
                {
                    TimesText += "( " + time.Start + " ) :: " + FormatTime(time.Time) + "\n"; // Format the time for better readability
                }
            }

            if (TimesText == "")
            {
                TimesText = "No times for this checkpoint.";
            }

            return TimesText;
        }
    }
}