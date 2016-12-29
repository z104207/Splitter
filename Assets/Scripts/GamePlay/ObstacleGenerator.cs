﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObstacleGenerator : MonoBehaviour {

	[System.Serializable]
	public class Obstacles{
		public GameObject spikeLeft;
		public GameObject spikeRight;
		public GameObject spikeBoth;
		public GameObject spikeCenter;
		public GameObject spikeThree;
	}

	public Obstacles obstacles;
	
	private Dictionary<int,ObjectPooler> objectPoolers;
	private int spikeId = 0;
	private bool isGameOver = false;

	void Start () {
		Events.GameOverEvent += GameOver;
		StartCoroutine(GenerateObstacle());
	}
	
	void OnDisable(){
		Events.GameOverEvent -= GameOver;
	}

	private IEnumerator GenerateObstacle(){
		objectPoolers = new Dictionary<int, ObjectPooler>(5);
		InitObjectPools();

		//make queue for storing previous values which will help in generating next spike.
		SpecialQueue queue = new SpecialQueue();
		spikeId = GetspikeId(queue);

		yield return new WaitForSeconds(1.8f);

		float waitTime = 1.8f;

		while(!isGameOver){
			Spawn(objectPoolers[spikeId].GetPooledObject());

			//Generate the next spike id and decide its generation rate.
			spikeId = GetspikeId(queue);

			if(queue.IsCenterSpike()) waitTime = waitTime + 0.3f;

			yield return new WaitForSeconds(waitTime);
			waitTime -= Random.Range(0, 0.2f);
			if (waitTime < 0.8f) {
				waitTime = 0.8f;
			}
		}
		
	}

	private void InitObjectPools(){
		objectPoolers[1] = new ObjectPooler(obstacles.spikeLeft, 3);
		objectPoolers[2] = new ObjectPooler(obstacles.spikeRight, 3);
		objectPoolers[3] = new ObjectPooler(obstacles.spikeBoth, 3);
		objectPoolers[4] = new ObjectPooler(obstacles.spikeCenter, 2);
		objectPoolers[5] = new ObjectPooler (obstacles.spikeThree, 3);
	}

	// Generates the new spike id that is to be instatiated.
	private int GetspikeId(SpecialQueue queue){
		int x = Random.Range(1, 6);
		if(queue.IsAlert(x)){
			int y = Random.Range(1, 5);
			if(x == y) x = 4;
			else x = y;
		}
		queue.Push(x);

		return x;
	}

	private GameObject Spawn(GameObject obj){
		Transform transForm = obj.transform;
		transForm.position = new Vector2(transForm.position.x, 12f);
		obj.SetActive(true);

		return obj;
	}

	private void GameOver(){
		//stopping all the spikes.
		if(!isGameOver){
			isGameOver = true;
		}
	}


	// Data Struct for holding previous generated values and provide useful to generate next spike.
	private class SpecialQueue{

		private readonly int [] array;

		public SpecialQueue(){
			array = new int[2];
			array[0] = array[1] = -1;
		}

		public void Push(int num){
			array[1] = array[0];
			array[0] = num;
		}

		// returns true is any spike (i.e 1,2,3) is going to be instatiated 3 times or 4th one 2 times in a row. 
		public bool IsAlert(int num){
			if(array[0] == 4 && num == 4){
				return true;
			}
			return (array[0]== array[1] && array[0] == num);
		}

		// returns true if next spike coming is 4th one after 1st or 2nd one.
		public bool IsCenterSpike(){
			return (array[0]==4 && array[1] <3);
		}
	}
}
