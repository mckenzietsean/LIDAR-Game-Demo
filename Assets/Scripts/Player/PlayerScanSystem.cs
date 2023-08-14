using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class PlayerScanSystem : MonoBehaviour
{
    [Header("References")]
    public PlayerStats playerStats;
    public Camera cam;
    public Transform scanPoint;
    public Dot scanPixel;
    private Gradient greenGradient = new Gradient();
    private Gradient redGradient = new Gradient();

    Vector3 viewPoint = new Vector3(0.5f, 0.5f, 0);
    Vector3 targetPoint;
    Vector3 directionWithoutSpread;
    Vector3 directionWithSpread;
    RaycastHit hit;

    public bool isScanning = false;
    public LayerMask scanMasks;
    public int totalScans = 0;

    public int maxScanDots = 5000;
    public ObjectPool dotPoolList;

    [Header("Scan Lines")]
    public float lineDuration = 0.01f;
    public LineRenderer line;

    [Header("Point Scan")]
    public float radius = 4f;
    public float radiusScale = 0.5f;
    public float pointScanTime = 0.01f;

    [Header("Wide Scan")]
    public float wideScanTime = 5f;
    public int maxCoroutines = 12;

    [Header("SFX")]
    public AudioSource pointScanSFX;
    public ScanAudio wideScanSFX;

    [Header("Input")]
    public KeyCode pointScanKey = KeyCode.Mouse0;
    public KeyCode wideScanKey = KeyCode.Mouse1;
    public KeyCode resetKey = KeyCode.R;

    // Start is called before the first frame update
    void Start()
    {
        float alpha = 1.0f;
        greenGradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.green, 0.0f), new GradientColorKey(Color.green, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f) }
        );
        redGradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.red, 0.0f), new GradientColorKey(Color.red, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f) }
        );

        dotPoolList = ObjectPool.CreateInstance(scanPixel, maxScanDots);
        HideScanLine();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isScanning && Input.GetKey(pointScanKey) && playerStats.canMove)
        {
            isScanning = true;
            PointScan();
            Invoke(nameof(EnableScanningAgain), pointScanTime);
        }

        if (!isScanning && Input.GetKey(wideScanKey) && playerStats.canMove)
        {
            isScanning = true;
            /*for(int i = 0; i < maxCoroutines; i++)
            {
                StartCoroutine(WiderScan(i));
            }*/


            _ = StartWideScan();
        }

        if(!isScanning && Input.GetKeyUp(resetKey) && playerStats.canMove)
        {
            dotPoolList.DisableAll();
            totalScans = 0;
        }

        AdjustRadius(); 
    }

    private void AdjustRadius()
    {
        if (Input.GetAxis("Mouse ScrollWheel") == 0)
            return;

        radius += Input.GetAxis("Mouse ScrollWheel") * radiusScale;
      
        if (radius > 0.4)
            radius = 0.4f;
        else if (radius < 0)
            radius = 0;
    }

    private void EnableScanningAgain()
    {
        isScanning = false;
    }

    public void HideScanLine()
    {
        line.enabled = false;
    }

    private void PointScan()
    {
        // Calculate spread
        directionWithSpread = cam.transform.forward + new Vector3(Random.Range(-radius, radius), Random.Range(-radius, radius), Random.Range(-radius, radius));

        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(cam.transform.position, directionWithSpread, out hit, 75f, scanMasks))
        {
            pointScanSFX.Play();
            line.enabled = true;
            line.SetPosition(0, scanPoint.position);
            line.SetPosition(1, hit.point);
            line.startColor = Color.green;
            line.endColor = Color.green;
            Debug.Log(hit.collider.tag);
            SpawnScanPixel(hit.point, Quaternion.LookRotation(hit.normal), hit.collider.tag);
            Invoke(nameof(HideScanLine), pointScanTime);   
        }
        else
        {
            line.enabled = true;
            line.SetPosition(0, scanPoint.position);
            line.SetPosition(1, transform.TransformDirection(directionWithSpread) * 75f);
            line.startColor = Color.red;
            line.endColor = Color.red;
            Invoke(nameof(HideScanLine), pointScanTime);
        }
    }

    private void SpawnScanPixel(Vector3 hitPos, Quaternion hitAngle, string hitTag)
    {
        PoolableObject po = dotPoolList.GetObject();
        po.transform.position = hitPos;
        po.transform.rotation = hitAngle;
        po.GetComponent<Dot>().ChangeColour(hitTag);
        totalScans++;
    }

    private async Task StartWideScan()
    {
        wideScanSFX.StartClip();
        var tasks = new Task[maxCoroutines];
        for(int i = 0; i < maxCoroutines; i++)
        {
            tasks[i] = WideScan(i);
        }
        await Task.WhenAll(tasks);

        Debug.Log("FINISHED.");
        EnableScanningAgain();
    }

    private async Task WideScan(int inc)
    {
        int maxPerRow = 60;
        float increments = 8;
        Vector3 corner = new Vector3((Screen.width / 2) - increments * maxPerRow / 2, (Screen.height / 2) + increments * maxPerRow / 2, 0);

        //Debug.Log(inc);

        // Rows
        for (int i = 0; i < maxPerRow; i++)
        {
            // Do for last row
            if (inc == 0 && i == maxPerRow - 1)
                wideScanSFX.EndClip();

            // Columns
            for (int j = inc; j < maxPerRow; j += maxCoroutines)
            {
                Ray r = cam.ScreenPointToRay(corner + new Vector3(j * increments + Random.Range(-2f, 2f), -i * increments + Random.Range(-2f, 2f), 0));
                targetPoint = r.GetPoint(75);
                directionWithSpread = targetPoint - cam.transform.position;
                if (Physics.Raycast(cam.transform.position, directionWithSpread, out hit, 75f, scanMasks))
                {
                    line.enabled = true;
                    line.SetPosition(0, scanPoint.position);
                    line.SetPosition(1, hit.point);
                    line.startColor = Color.green;
                    line.endColor = Color.green;
                    SpawnScanPixel(hit.point, Quaternion.LookRotation(hit.normal), hit.collider.tag);
                }
                else
                {

                    line.enabled = true;
                    line.SetPosition(0, scanPoint.position);
                    line.SetPosition(1, transform.TransformDirection(directionWithSpread) * 75f);
                    line.startColor = Color.red;
                    line.endColor = Color.red;
                    Invoke(nameof(HideScanLine), pointScanTime);
                }
                await Task.Yield();
            }

            
        }

        //yield return new WaitForSeconds(2f);

        Invoke(nameof(HideScanLine), pointScanTime);
        isScanning = false;
    }

    private IEnumerator WiderScan(int inc)
    {
        int maxPerRow = 60;
        float increments = 8;
        Vector3 corner = new Vector3((Screen.width / 2) - increments*maxPerRow/2, (Screen.height / 2) + increments * maxPerRow / 2, 0);

        //Debug.Log(inc);

        // Rows
        for(int i = 0; i < maxPerRow; i++)
        {
            // Columns
            for(int j = inc; j < maxPerRow; j += maxCoroutines)
            {
                Ray r = cam.ScreenPointToRay(corner + new Vector3(j*increments + Random.Range(-2f, 2f), -i*increments + Random.Range(-2f, 2f), 0));
                targetPoint = r.GetPoint(75);
                directionWithSpread = targetPoint - cam.transform.position;
                if (Physics.Raycast(cam.transform.position, directionWithSpread, out hit, 75f, scanMasks))
                {
                    line.enabled = true;
                    line.SetPosition(0, scanPoint.position);
                    line.SetPosition(1, hit.point);
                    line.startColor = Color.green;
                    line.endColor = Color.green;
                    SpawnScanPixel(hit.point, Quaternion.LookRotation(hit.normal), hit.collider.tag);
                }
                else
                {

                    line.enabled = true;
                    line.SetPosition(0, scanPoint.position);
                    line.SetPosition(1, transform.TransformDirection(directionWithSpread) * 75f);
                    line.startColor = Color.red;
                    line.endColor = Color.red;
                    Invoke(nameof(HideScanLine), pointScanTime);
                }
                yield return new WaitForSeconds(0.0f);
            }

            
        }

        //yield return new WaitForSeconds(2f);

        Invoke(nameof(HideScanLine), pointScanTime);
        isScanning = false;
    }
}
