using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RobotEnergyUI : MonoBehaviour {
	//Battery Level Slider/Bar holder
	public Slider batteryLevelSlider;
	public Gradient batteryLevelGradient;
	public Image batteryLevelFill;
	public Text batteryVoltageText;

	public Slider batteryCurrentSlider;
	public Text batteryCurrentText;

	public Text batteryStatusText;

	public void SetBatteryLevel(int level) {
		batteryLevelSlider.value = level;

		batteryLevelFill.color = batteryLevelGradient.Evaluate(batteryLevelSlider.normalizedValue);
	}

	public void SetBatteryVoltage(float voltage) {
		batteryVoltageText.text = voltage.ToString("F2") + " V";
	}

	public void SetBatteryCurrent(int current) {
		batteryCurrentSlider.value = current;

		batteryCurrentText.text = current.ToString() + " mA";
	}

	public void SetBatteryStatus(int status) {
		if (status == 0) {
			batteryStatusText.text = "Discharging";
		}
		else if(status == 1) {
			batteryStatusText.text = "Charging";
		}
	}
}
