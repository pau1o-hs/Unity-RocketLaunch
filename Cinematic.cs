using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cinematic : MonoBehaviour
{
	[System.Serializable]
	public struct ThrustSettings
	{
		public float force;
		public float angle;
		public float time;
	}

	[System.Serializable]
	public struct TimeSettings
	{
		public float time;
		public float speed;
		public float max;
	}

	public Rigidbody2D[] planet;
	public ThrustSettings[] propellant;
	public TimeSettings time;
	public Manager_UI ui;

	Rigidbody2D rocket;
    	Vector2 startPosition;

	float startAngle;
	float[] planetDistance = new float[2];

	bool launched;

	// Start is called before the first frame update
	void Start()
    	{
		rocket = GetComponent<Rigidbody2D>();

        	startPosition = rocket.position;
		startAngle = rocket.rotation;

		rocket.velocity = Vector2.zero;

		ResetSimulation();
	}

    // Update is called once per frame
    void FixedUpdate()
    {
		Time.timeScale = time.speed;
		time.time += Time.deltaTime;

		if (planetDistance[0] > (planet[0].position - startPosition).magnitude)
		{
			launched = true;
		}

		Thrust();
		Gravity();

		UI();
	}

	void Thrust()
	{
		// THRUST 1
		if (time.time <= propellant[0].time)
		{
			rocket.AddForce(propellant[0].force * transform.right, ForceMode2D.Force);
			rocket.AddTorque(propellant[0].angle, ForceMode2D.Force);

			ui.propellant[0].fuel.value = 1 - (time.time / propellant[0].time);
		}

		// THRUST 2
		else if (time.time <= propellant[0].time + propellant[1].time)
		{
			rocket.AddForce(propellant[1].force * transform.right, ForceMode2D.Force);
			rocket.AddTorque(propellant[1].angle, ForceMode2D.Force);

			ui.propellant[1].fuel.value = 1 - ((time.time - propellant[0].time) / propellant[1].time);
		}

		// THRUST 3
		else if (time.time <= propellant[0].time + propellant[1].time + propellant[2].time)
		{
			rocket.AddForce(propellant[2].force * transform.right, ForceMode2D.Force);
			rocket.AddTorque(propellant[2].angle, ForceMode2D.Force);

			ui.propellant[2].fuel.value = 1 - ((time.time - propellant[0].time - propellant[1].time) / propellant[2].time);
		}
	}

	void Gravity()
	{
		for (int i = 0; i < planet.Length; i++)
		{
			Vector2 direction = planet[i].position - rocket.position;
			planetDistance[i] = direction.magnitude;

			if (planet[i].GetComponentInChildren<Draw_Circle>().lowOrbit - planetDistance[i] < 0) continue;

			float forceMagnitude = 6.673f * (planet[i].mass * rocket.mass) / Mathf.Pow(planetDistance[i], 2);
			Vector2 force = direction.normalized * forceMagnitude;

			rocket.AddForce(force, ForceMode2D.Force);
		}
	}

	void UI()
	{
		ui.logVelocity.text = "Velocity: " + "<b>" + (rocket.velocity.magnitude).ToString("F2") + " m/s</b>";
		ui.logAngVelocity.text = "Angular velocity: " + "<b>" + rocket.angularVelocity.ToString("F2") + " deg/s</b>";
		ui.logHeight.text = "Altitude: " + "<b>" + (rocket.position.magnitude - 6378f).ToString("F2") + " m</b>";
		ui.logTimer.text = (Mathf.Floor(time.time / 3600.0f)).ToString("00") + ":" + (Mathf.Floor(time.time / 60.0f) % 59.0f).ToString("00") + ":" + (time.time % 59.0f).ToString("00");

		for (int i = 0; i < propellant.Length; i++)
		{
			ui.propellant[i].thrust.value = int.Parse(ui.propellant[i].valueThrust.text);
			ui.propellant[i].torque.value = float.Parse(ui.propellant[i].valueTorque.text);
			ui.propellant[i].time.value = float.Parse(ui.propellant[i].valueTime.text);
		}
	}

	public void ResetSimulation()
	{
		rocket.position = startPosition;
		rocket.rotation = startAngle;

		rocket.velocity = Vector2.zero;
		rocket.angularVelocity = 0;

		time.time = 0;
		launched = false;

		for (int i = 0; i < propellant.Length; i++)
		{
			propellant[i].force = ui.propellant[i].thrust.value;
			propellant[i].angle = ui.propellant[i].torque.value;
			propellant[i].time = ui.propellant[i].time.value;
		}

		ui.propellant[0].fuel.value = 1;
		ui.propellant[1].fuel.value = 1;
		ui.propellant[2].fuel.value = 1;
	}

	void OnCollisionEnter2D(Collision2D other)
	{
		if (launched && (other.transform.name == "Earth" || other.transform.name == "Moon"))
			ResetSimulation();
	}
}
