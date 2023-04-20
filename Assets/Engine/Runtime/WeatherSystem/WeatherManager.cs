using System.Collections.Generic;
using CFClient.React;
using UnityEngine;

namespace CFEngine.WeatherSystem
{
    public class WeatherManager
    {
        private static WeatherManager _instance;

        public static WeatherManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new WeatherManager();
                }

                return _instance;
            }
        }
        
        private List<WeatherBehaviour> _weatherBehaviourList = new List<WeatherBehaviour>();

        public WeatherManager()
        {
            WeatherSystemExtend.Instance.OnWeatherParamChanged += OnWeatherParamChange;
        }

        ~WeatherManager()
        {
            WeatherSystemExtend.Instance.OnWeatherParamChanged -= OnWeatherParamChange;
        }

        public void OnWeatherParamChange(WeatherSystemExtend.WeatherParams weatherParams)
        {
            if (_weatherBehaviourList == null)
            {
                return;
            }

            for (int i = 0; i < _weatherBehaviourList.Count; i++)
            {
                _weatherBehaviourList[i].SetWeatherParams(weatherParams);
            }
        }

        

        public void Register(WeatherBehaviour weatherBehaviour)
        {
            if (_weatherBehaviourList == null)
            {
                Debug.Log("_weatherBehaviourList 为空");
                return;
            }

            if (_weatherBehaviourList.Contains(weatherBehaviour))
            {
                return;
            }

            _weatherBehaviourList.Add(weatherBehaviour);
        }

        public void UnRegister(WeatherBehaviour weatherBehaviour)
        {
            if (_weatherBehaviourList == null)
            {
                Debug.Log("_weatherBehaviourList 为空");
                return;
            }

            if (_weatherBehaviourList.Contains(weatherBehaviour))
            {
                return;
            }

            _weatherBehaviourList.Remove(weatherBehaviour);
        }
    }
}