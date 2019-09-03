using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using VacantLot.VLUtil;

namespace VacantLot.VLVTuber
{

    public class SceneryChanger : MonoBehaviour
    {
        [SerializeField] SceneObject[] Sceneries;
        [SerializeField] int DefaultSceneryIndex = 0;

        void Start()
        {
            Current = Sceneries[DefaultSceneryIndex];
        }

        public void LoadScenery(int index) => Current = Sceneries[index];
        public bool IsSceneryChanging { get; private set; }

        SceneObject _Current = null;
        SceneObject Current
        {
            get { return _Current; }
            set { StartCoroutine(ChangeScene(value)); }
        }

        IEnumerator ChangeScene(SceneObject scenery)
        {
            if (IsSceneryChanging) yield break;
            if (_Current == scenery) yield break;

            IsSceneryChanging = true;

            RemoveScene(_Current);
            while (IsUnloading) yield return null;
            AddScene(scenery);
            while (IsLoading) yield return null;

            _Current = scenery;
            IsSceneryChanging = false;
            Debug.Log(_Current + " is loaded.");
        }

        void AddScene(SceneObject scenery)
        {
            if (!string.IsNullOrEmpty(scenery)) _LoadOperation = SceneManager.LoadSceneAsync(scenery, LoadSceneMode.Additive);
        }

        void RemoveScene(SceneObject scenery)
        {
            if (!string.IsNullOrEmpty(scenery)) _UnloadOperation = SceneManager.UnloadSceneAsync(scenery);
        }

        AsyncOperation _UnloadOperation;
        AsyncOperation _LoadOperation;

        bool IsLoading => !(_LoadOperation?.isDone ?? true);
        bool IsUnloading => !(_UnloadOperation?.isDone ?? true);
    }
}