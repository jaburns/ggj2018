using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FadeCameraController : MonoBehaviour 
{
    static FadeCameraController instance;

    static Color color = Color.white;

    Camera camera;
    Material material;

    static bool canDoStuff = true;

    void Awake()
    {
        camera = GetComponent<Camera>();
        material = GetComponentInChildren<Renderer>().sharedMaterial;
        instance = this;
        StartCoroutine(fadeIn());
    }

    static public void Win()
    {
        if (!canDoStuff) return;
        canDoStuff = false;
        color = Color.white;
        instance.StartCoroutine(fadeOut(() => {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            canDoStuff = true;
        }));
    }

    static public void Lose()
    {
        if (!canDoStuff) return;
        Audio.Play("Death");
        canDoStuff = false;
        color = Color.black;
        instance.StartCoroutine(fadeOut(() => {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            canDoStuff = true;
        }));
    }

    static IEnumerator fadeIn()
    {
        color.a = 1f;
        instance.camera.enabled = true;
        instance.material.SetColor("_Color",color);

        for (var t = 1f; t > 0f; t -= Time.deltaTime) {
            yield return new WaitForEndOfFrame();

            color.a = t;
            instance.material.SetColor("_Color",color);
        }

        instance.camera.enabled = false;
    }

    static IEnumerator fadeOut( Action after)
    {
        Debug.Log(color);
        color.a = 0f;
        instance.camera.enabled = true;
        instance.material.SetColor("_Color",color);

        for (var t = 0f; t < 1f; t += Time.deltaTime) {
            yield return new WaitForEndOfFrame();

            color.a = t;
            instance.material.SetColor("_Color",color);
        }

        after();
    }
}
