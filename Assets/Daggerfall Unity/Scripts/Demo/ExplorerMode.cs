using UnityEngine;
using System.Collections;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using DaggerfallConnect.Utility;

namespace DaggerfallWorkshop.Demo
{
    /// <summary>
    /// Some basic methods for dfworkshop.net demos.
    /// </summary>
    public class ExplorerMode : MonoBehaviour
    {
        public PlayerWeather playerWeather;

        DaggerfallUnity dfUnity;
        StreamingWorld streamingWorld;
        SceneFadeInOut sceneFader;
        DaggerfallSongPlayer songPlayer;

        int timeScaleControl = 1;
        int minTimeScaleControl = 1;
        int maxTimeScaleControl = 150;
        int timeScaleStep = 25;
        float timeScaleMultiplier = 10f;

        int songIndex = (int)SongFilesGM.song_03;
        int minSongIndex = 0;
        int maxSongIndex = SongFilesGM.GetValues(typeof(SongFilesGM)).Length - 1;

        void Start()
        {
            dfUnity = DaggerfallUnity.Instance;
            streamingWorld = GameObject.FindObjectOfType<StreamingWorld>();
            sceneFader = GameObject.FindObjectOfType<SceneFadeInOut>();
            songPlayer = GameObject.FindObjectOfType<DaggerfallSongPlayer>();
            songPlayer.Song = SongFilesAll.song_03;
        }

        void Update()
        {
            if (!streamingWorld.IsInit)
                sceneFader.ShowLoadTexture = false;

            // Random location
            if (Input.GetKeyDown(KeyCode.R))
            {
                sceneFader.ShowLoadTexture = true;
                TeleportRandomLocation();
            }

            // Preset locations
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                sceneFader.ShowLoadTexture = true;
                TeleportLocation("Daggerfall", "Daggerfall");
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                sceneFader.ShowLoadTexture = true;
                TeleportLocation("Wayrest", "Wayrest");
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                sceneFader.ShowLoadTexture = true;
                TeleportLocation("Sentinel", "Sentinel");
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                sceneFader.ShowLoadTexture = true;
                TeleportLocation("Orsinium Area", "Orsinium");
            }

            // Time scale
            if (Input.GetKeyDown(KeyCode.Equals))
            {
                timeScaleControl += timeScaleStep;
                if (timeScaleControl > maxTimeScaleControl)
                    timeScaleControl = maxTimeScaleControl;
                dfUnity.WorldTime.TimeScale = timeScaleControl * timeScaleMultiplier;
            }
            if (Input.GetKeyDown(KeyCode.Minus))
            {
                timeScaleControl -= timeScaleStep;
                if (timeScaleControl < minTimeScaleControl)
                    timeScaleControl = minTimeScaleControl;
                dfUnity.WorldTime.TimeScale = timeScaleControl * timeScaleMultiplier;
            }

            // Music control
            if (Input.GetKeyDown(KeyCode.P))
            {
                SongFilesGM songFile = (SongFilesGM)songIndex;
                if (!songPlayer.IsPlaying)
                    songPlayer.Play(songFile.ToString());
            }
            if (Input.GetKeyDown(KeyCode.RightBracket))
            {
                int lastSongIndex = songIndex;
                songIndex++;
                if (songIndex > maxSongIndex)
                    songIndex = maxSongIndex;

                SongFilesGM songFile = (SongFilesGM)songIndex;
                if (songIndex != lastSongIndex)
                    songPlayer.Play(songFile.ToString());
            }
            if (Input.GetKeyDown(KeyCode.LeftBracket))
            {
                int lastSongIndex = songIndex;
                songIndex--;
                if (songIndex < minSongIndex)
                    songIndex = minSongIndex;

                SongFilesGM songFile = (SongFilesGM)songIndex;
                if (songIndex != lastSongIndex)
                    songPlayer.Play(songFile.ToString());
            }
        }

        // Teleport player to any location by name
        public void TeleportLocation(string regionName, string locationName)
        {
            DFLocation location = dfUnity.ContentReader.MapFileReader.GetLocation(regionName, locationName);
            if (!location.Loaded)
                return;

            // Check inside range
            DFPosition mapPos = MapsFile.LongitudeLatitudeToMapPixel((int)location.MapTableData.Longitude, (int)location.MapTableData.Latitude);
            if (mapPos.X >= TerrainHelper.minMapPixelX || mapPos.X < TerrainHelper.maxMapPixelX ||
                mapPos.Y >= TerrainHelper.minMapPixelY || mapPos.Y < TerrainHelper.maxMapPixelY)
            {
                streamingWorld.TeleportToCoordinates(mapPos.X, mapPos.Y);
            }
        }

        // Teleports player to a random location in a random region
        public void TeleportRandomLocation()
        {
            DFPosition mapPos = new DFPosition();
            bool found = false;
            while (!found)
            {
                // Get random region
                int regionIndex = UnityEngine.Random.Range(0, dfUnity.ContentReader.MapFileReader.RegionCount);
                DFRegion region = dfUnity.ContentReader.MapFileReader.GetRegion(regionIndex);
                if (region.LocationCount == 0)
                    continue;

                // Get random location
                int locationIndex = UnityEngine.Random.Range(0, region.MapTable.Length);
                DFLocation location = dfUnity.ContentReader.MapFileReader.GetLocation(regionIndex, locationIndex);
                if (!location.Loaded)
                    continue;

                // Check inside range
                mapPos = MapsFile.LongitudeLatitudeToMapPixel((int)location.MapTableData.Longitude, (int)location.MapTableData.Latitude);
                if ((mapPos.X >= TerrainHelper.minMapPixelX + 2 && mapPos.X < TerrainHelper.maxMapPixelX - 2) &&
                    (mapPos.Y >= TerrainHelper.minMapPixelY + 2 && mapPos.Y < TerrainHelper.maxMapPixelY - 2))
                {
                    found = true;
                }
            }

            // Teleport
            streamingWorld.TeleportToCoordinates(mapPos.X, mapPos.Y);
        }
    }
}