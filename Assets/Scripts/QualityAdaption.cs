﻿
using UnityEngine;
using System.Collections;

/// <summary>
/// Automatically scales quality up or down based on the current framerate (average).
/// </summary>
/// 
/// \author Kaspar Manz
/// \date 2014-03-10
/// \version 1.0.0
public class QualityAdaption : MonoBehaviour
{
	public static QualityAdaption qualitySetter;
	/// <summary>
	/// The number of data points to calculate the average FPS over.
	/// </summary>
	int numberOfDataPoints;
	/// <summary>
	/// The current average fps.
	/// </summary>
	float currentAverageFps;
	/// <summary>
	/// The time interval in which the class checks for the framerate and adapts quality accordingly.
	/// </summary>
	public float TimeIntervalToAdaptQualitySettings;
	/// <summary>
	/// The lower FPS threshold. Decrease quality when FPS falls below this.
	/// </summary>
	public float LowerFPSThreshold;
	/// <summary>
	/// The upper FPS threshold. Increase quality when FPS is above this.
	/// </summary>
	public float UpperFPSThreshold;
	/// <summary>
	/// The stability of the current quality setting. Below 0 if changes have been
	/// made, otherwise positive.
	/// </summary>
	int stability;
	/// <summary>
	/// Tracks whether quality was improved or worsened.
	/// </summary>
	bool lastMovementWasDown;
	/// <summary>
	/// Counter that keeps track when the script can't decide between lowering or increasing quality.
	/// </summary>
	int flickering;
	int prevousSetting;


	void Awake(){
		if (qualitySetter != null || GameController.control.timesPlayedToday > 0) {
			Destroy (gameObject);
			Debug.Log ("Already have one, KILLING");
		} else {
			Debug.Log ("Starting Quality Setter");
			Debug.Log ("Current Setting = " + QualitySettings.names [QualitySettings.GetQualityLevel ()]);
			Debug.Log ("Current FPS = " + currentAverageFps);
			qualitySetter = this;
			GameObject.DontDestroyOnLoad (gameObject);

		}
	}

	void Start ()
	{
		StartCoroutine (AdaptQuality ());
	}


	void Update ()
	{
		UpdateCumulativeAverageFPS (1 / Time.deltaTime);
	}


	/// <summary>
	/// Updates the cumulative average FPS.
	/// </summary>
	/// <param name="newFPS">New FPS.</param>
	float UpdateCumulativeAverageFPS (float newFPS)
	{
		++numberOfDataPoints;
		currentAverageFps += (newFPS - currentAverageFps) / numberOfDataPoints;

		return currentAverageFps;
	}


	/// <summary>
	/// Sets the quality accordingly to the current thresholds.
	/// </summary>
	IEnumerator AdaptQuality ()
	{
		while (true) {
			yield return new WaitForSeconds (TimeIntervalToAdaptQualitySettings);

			if (Debug.isDebugBuild) {
				Debug.Log ("Current Average Framerate is: " + currentAverageFps);
			}

			// Decrease level if framerate too low
			if (currentAverageFps < LowerFPSThreshold) {
				prevousSetting = QualitySettings.GetQualityLevel ();
				QualitySettings.DecreaseLevel ();

				--stability;
				if (!lastMovementWasDown) {
					++flickering;
				}
				lastMovementWasDown = true;
				if (Debug.isDebugBuild) {
					Debug.Log ("Reducing Quality Level, now " + QualitySettings.names [QualitySettings.GetQualityLevel ()]);
				}

				// In case we are "flickering" (switching between two quality settings),
				// stop it, using the lower quality level.
				if (flickering > 1 || QualitySettings.GetQualityLevel() == prevousSetting) {
					if (Debug.isDebugBuild) {
						Debug.Log (string.Format (
							"Flickering detected, staying at {0} to stabilise.",
							QualitySettings.names [QualitySettings.GetQualityLevel ()]));
					}
					Debug.Log ("At Lowest: KILLING");
					Destroy (this);
				}

			} else  
				// Increase level if framerate is too high
				if (currentAverageFps > UpperFPSThreshold) {
					prevousSetting = QualitySettings.GetQualityLevel ();
					QualitySettings.IncreaseLevel ();
					--stability;
					if (lastMovementWasDown) {
						++flickering;
					}
					lastMovementWasDown = false;
					if (Debug.isDebugBuild) {
						Debug.Log ("Increasing Quality Level, now " + QualitySettings.names [QualitySettings.GetQualityLevel ()]);
					}

					if (QualitySettings.GetQualityLevel() == prevousSetting) {
						//we have didnt get any higher so stop the process
						Debug.Log ("At Highest, KILLING NOW");
						Destroy(gameObject);
					}
				} else {
					++stability;
				}

			// If we had a framerate in the range between 25 and 60 frames three times
			// in a row, we consider this pretty stable and remove this script.
			if (stability > 3) {
				if (Debug.isDebugBuild) {
					Debug.Log ("Framerate is stable now, removing automatic adaptation.");
				}
				Debug.Log ("Framerate is stable now, removing automatic adaptation.");
				Destroy (this);
			}

			// Reset moving average
			numberOfDataPoints = 0;
			currentAverageFps = 0;
		}
	}
}