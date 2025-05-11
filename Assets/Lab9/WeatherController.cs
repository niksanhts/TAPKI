using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.PostProcessing;

public class WeatherController : MonoBehaviour
{
    public enum Season { Spring, Summer, Autumn, Winter }
    public enum WeatherEffect { None, Rain, Snow, Fog }

    [Header("Time Settings")]
    [SerializeField] private Light sun;
    [SerializeField] private float dayDuration = 120f;
    [SerializeField] private Gradient skyGradient;
    [SerializeField] private AnimationCurve lightIntensityCurve;

    [Header("Season Settings")]
    [SerializeField] private ParticleSystem rainParticles;
    [SerializeField] private ParticleSystem snowParticles;
    [SerializeField] private ParticleSystem fogParticles;
    [SerializeField] private float seasonDuration = 60f;
    [SerializeField] private PostProcessVolume postProcessVolume;
    [SerializeField] private PostProcessProfile[] seasonPostEffects;

    [Header("Effects Settings")]
    [SerializeField] private float minRandomEffectTime = 30f;
    [SerializeField] private float maxRandomEffectTime = 60f;
    [SerializeField] private float effectTransitionTime = 2f;
    [SerializeField] private float windForce = 0.5f;

    private Season currentSeason;
    private WeatherEffect currentWeather;
    private float timeOfDay;
    private Material skyboxMaterial;
    private Coroutine[] activeCoroutines;

    private void Start()
    {
        InitializeSystems();
        StartAllCycles();
    }

    private void InitializeSystems()
    {
        skyboxMaterial = RenderSettings.skybox;
        activeCoroutines = new Coroutine[3];
        SetInitialSeason();
        InitializeParticles();
    }

    private void SetInitialSeason()
    {
        currentSeason = Season.Spring;
        UpdatePostProcessing();
    }

    private void InitializeParticles()
    {
        StopAllParticles();
        SetParticlesWind();
    }

    private void StartAllCycles()
    {
        activeCoroutines[0] = StartCoroutine(TimeOfDayCycle());
        activeCoroutines[1] = StartCoroutine(SeasonCycle());
        activeCoroutines[2] = StartCoroutine(RandomWeatherEffects());
    }

    private IEnumerator TimeOfDayCycle()
    {
        while (true)
        {
            UpdateDayNightCycle();
            yield return null;
        }
    }

    private void UpdateDayNightCycle()
    {
        timeOfDay = Mathf.Repeat(timeOfDay + Time.deltaTime / dayDuration, 1f);
        UpdateLighting();
        UpdateSunIntensity();
    }

    private void UpdateLighting()
    {
        sun.transform.rotation = Quaternion.Euler(timeOfDay * 360f - 90f, 0f, 0f);
        RenderSettings.ambientSkyColor = skyGradient.Evaluate(timeOfDay);
        skyboxMaterial.SetColor("_Tint", skyGradient.Evaluate(timeOfDay));
    }

    private void UpdateSunIntensity()
    {
        sun.intensity = lightIntensityCurve.Evaluate(timeOfDay);
    }

    private IEnumerator SeasonCycle()
    {
        while (true)
        {
            yield return new WaitForSeconds(seasonDuration);
            SwitchToNextSeason();
            UpdateSeasonEffects();
        }
    }

    private void SwitchToNextSeason()
    {
        currentSeason = (Season)(((int)currentSeason + 1) % 4);
        UpdatePostProcessing();
        SetParticlesWind();
    }

    private void UpdatePostProcessing()
    {
        if (postProcessVolume && seasonPostEffects.Length >= 4)
        {
            postProcessVolume.profile = seasonPostEffects[(int)currentSeason];
        }
    }

    private void UpdateSeasonEffects()
    {
        switch (currentSeason)
        {
            case Season.Winter:
                SetWeather(WeatherEffect.Snow, effectTransitionTime);
                break;
            case Season.Summer:
                SetWeather(WeatherEffect.Rain, effectTransitionTime);
                break;
            case Season.Spring:
                SetWeather(WeatherEffect.Fog, effectTransitionTime);
                break;
            case Season.Autumn:
                SetWeather(WeatherEffect.None, effectTransitionTime);
                break;
        }
    }

    private IEnumerator RandomWeatherEffects()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minRandomEffectTime, maxRandomEffectTime));
            WeatherEffect randomEffect = GetSeasonAppropriateEffect();
            SetWeather(randomEffect, effectTransitionTime);
        }
    }

    private WeatherEffect GetSeasonAppropriateEffect()
    {
        return currentSeason switch
        {
            Season.Winter => WeatherEffect.Snow,
            Season.Summer => WeatherEffect.Rain,
            _ => (WeatherEffect)Random.Range(1, System.Enum.GetValues(typeof(WeatherEffect)).Length)
        };
    }

    public void SetWeather(WeatherEffect effect, float transitionTime)
    {
        if (effect == currentWeather) return;
        StartCoroutine(TransitionWeather(effect, transitionTime));
    }

    private IEnumerator TransitionWeather(WeatherEffect newEffect, float transitionTime)
    {
        WeatherEffect oldEffect = currentWeather;
        currentWeather = newEffect;

        if (oldEffect != WeatherEffect.None)
        {
            yield return FadeParticles(GetParticleSystem(oldEffect), 0f, transitionTime);
        }

        if (newEffect != WeatherEffect.None)
        {
            ParticleSystem newSystem = GetParticleSystem(newEffect);
            newSystem.Play();
            yield return FadeParticles(newSystem, 1f, transitionTime);
        }
    }

    private ParticleSystem GetParticleSystem(WeatherEffect effect)
    {
        return effect switch
        {
            WeatherEffect.Rain => rainParticles,
            WeatherEffect.Snow => snowParticles,
            WeatherEffect.Fog => fogParticles,
            _ => null
        };
    }

    private IEnumerator FadeParticles(ParticleSystem system, float targetAlpha, float duration)
    {
        var main = system.main;
        ParticleSystem.MinMaxGradient startColor = main.startColor;
        float startAlpha = startColor.color.a;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            Color newColor = startColor.color;
            newColor.a = Mathf.Lerp(startAlpha, targetAlpha, timer / duration);
            main.startColor = newColor;
            yield return null;
        }

        if (targetAlpha == 0f) system.Stop();
    }

    private void SetParticlesWind()
    {
        var rainForce = rainParticles.forceOverLifetime;
        var snowForce = snowParticles.forceOverLifetime;
        var windDirection = new Vector3(Random.Range(-windForce, windForce), 0, Random.Range(-windForce, windForce));

        rainForce.x = windDirection.x;
        rainForce.z = windDirection.z;
        snowForce.x = windDirection.x;
        snowForce.z = windDirection.z;
    }

    private void StopAllParticles()
    {
        rainParticles.Stop();
        snowParticles.Stop();
        fogParticles.Stop();
    }

    private void OnDestroy()
    {
        foreach (Coroutine coroutine in activeCoroutines)
        {
            if (coroutine != null) StopCoroutine(coroutine);
        }
    }
}