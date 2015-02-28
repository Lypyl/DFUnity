// this file is a contribution to Daggerfall Tools For Unity
// Project:         Increased Terrain Distance for Daggerfall Tools For Unity
// Author:          Michael Rauter (a.k.a. _Nystul_ from reddit)
// File Version:	1.0
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)

// original project:
// Project:         Daggerfall Tools For Unity
// Copyright:       Copyright (C) 2015 Gavin Clayton
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Web Site:        http://www.dfworkshop.net
// Contact:         Gavin Clayton (interkarma@dfworkshop.net)
// Project Page:    https://github.com/Interkarma/daggerfall-unity

Shader "Daggerfall/IncreasedTerrainTilemap" {
	Properties {
		// These params are required to stop terrain system throwing errors
		// However we won't be using them as Unity likes to init these textures
		// and will overwrite any assignments we already made
		// TODO: Combine splat painting with tilemapping
		[HideInInspector] _MainTex("BaseMap (RGB)", 2D) = "white" {}
		[HideInInspector] _Control ("Control (RGBA)", 2D) = "red" {}
		[HideInInspector] _SplatTex3("Layer 3 (A)", 2D) = "white" {}
		[HideInInspector] _SplatTex2("Layer 2 (B)", 2D) = "white" {}
		[HideInInspector] _SplatTex1("Layer 1 (G)", 2D) = "white" {}
		[HideInInspector] _SplatTex0("Layer 0 (R)", 2D) = "white" {}

		// These params are used for our shader
		_TileAtlasTexDesert ("Tileset Atlas (RGB)", 2D) = "white" {}
		_TileAtlasTexWoodland ("Tileset Atlas (RGB)", 2D) = "white" {}
		_TileAtlasTexMountain ("Tileset Atlas (RGB)", 2D) = "white" {}
		_TileAtlasTexSwamp ("Tileset Atlas (RGB)", 2D) = "white" {}
		_TilemapTex("Tilemap (R)", 2D) = "red" {}
		_TilesetDim("Tileset Dimension (in tiles)", Int) = 16
		_TilemapDim("Tilemap Dimension (in tiles)", Int) = 1000
		_MaxIndex("Max Tileset Index", Int) = 255
		_AtlasSize("Atlas Size (in pixels)", Float) = 2048.0
		_GutterSize("Gutter Size (in pixels)", Float) = 32.0
		_TerrainDistanceInWorldUnits("Terrain Distance in world units", Float) = 2457.6
		_WaterHeightTransformed("water level on y-axis in world coordinates", Float) = 58.9
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Lambert
		#pragma glsl

		#define PI 3.1416f

		sampler2D _TileAtlasTexDesert;
		sampler2D _TileAtlasTexWoodland;
		sampler2D _TileAtlasTexMountain;
		sampler2D _TileAtlasTexSwamp;
		sampler2D _TilemapTex;
		int _TilesetDim;
		int _TilemapDim;
		int _MaxIndex;
		float _AtlasSize;
		float _GutterSize;
		float _WaterHeightTransformed;
		float _TerrainDistanceInWorldUnits;

		struct Input
		{
			float2 uv_MainTex;
			float3 worldPos; // interpolated vertex positions used for correct coast line texturing
			float3 worldNormal; // interpolated vertex normals used for texturing terrain based on terrain slope
		};
		
		half4 getColorByTextureAtlasIndex(Input IN, uniform sampler2D textureAtlas, int index)
		{			
			const float textureCrispness = 3.5f; // defines how crisp textures of extended terrain are (higher values result in more crispness)
			const float textureCrispnessDiminishingFactor = 0.075f; // defines how fast crispness of textures diminishes with more distance from the player (the camera)
			const float distanceAttenuation = 0.001; // used to attenuated computed distance

			float dist = max(abs(IN.worldPos.x - _WorldSpaceCameraPos.x), abs(IN.worldPos.z - _WorldSpaceCameraPos.z));
			dist = floor(dist*distanceAttenuation);

			int xpos = index % _TilesetDim;
			int ypos = index / _TilesetDim;
			float2 uv = float2(xpos, ypos) / _TilesetDim;

			// Offset to fragment position inside tile
			float xoffset;
			float yoffset;
			// changed offset computation so that tile texture repeats over tile
			xoffset = frac(IN.uv_MainTex.x * _TilemapDim * 1/(max(1,dist * textureCrispnessDiminishingFactor)) * textureCrispness ) / _GutterSize;
			yoffset = frac(IN.uv_MainTex.y * _TilemapDim * 1/(max(1,dist * textureCrispnessDiminishingFactor)) * textureCrispness ) / _GutterSize;
			 
			uv += float2(xoffset, yoffset) + _GutterSize / _AtlasSize;

			// Sample based on gradient and set output
			float2 uvr = IN.uv_MainTex * ((float)_TilemapDim / _GutterSize);
			half4 c = tex2Dgrad(textureAtlas, uv, ddx(uvr), ddy(uvr));
			return(c);
		}

		void surf (Input IN, inout SurfaceOutput o)
		{
			const float limitAngleDirtTexture = 12.5f * PI / 180.0f; // tile will get dirt texture assigned if angles definied by surface normal and up-vector is larger than this value (and not larger than limitAngleStoneTexture)
			const float limitAngleStoneTexture = 20.5f * PI / 180.0f; // tile will get stone texture assigned if angles definied by surface normal and up-vector is larger than this value

			half4 c; // output color value
			
			half4 c_g; // color value from grass texture
			half4 c_d; // color value from dirt texture
			half4 c_s; // color value from stone texture

			float weightGrass = 1.0f; // weight factor for grass texture used for combining for final color value 
			float weightDirt = 0.0f; // weight factor for dirt texture used for combining for final color value
			float weightStone = 0.0f; // weight factor for stone texture used for combining for final color value

			int index = tex2D(_TilemapTex, IN.uv_MainTex).x * _MaxIndex;

			// there are several possibilities to get the tile surface normal...
			// float3 surfaceNormal = normalize(cross(ddx(IN.worldPos.xyz), ddy(IN.worldPos.xyz))); // approximate it from worldPosition with derivations			
			// float3 surfaceNormal = normalize(o.Normal); // interpolated vertex normal
			// float3 surfaceNormal = IN.worldNormal; // interpolated vertex normal (by input parameter)
			// float3 surfaceNormal = WorldNormalVector(IN, o.Normal); // don't know what the difference is (googled it - was mentioned that it does not get interpolated but i can't confirm this)
			// float3 surfaceNormal = 0.95f*(IN.worldNormal)+0.05f*normalize(cross(ddx(IN.worldPos.xyz), ddy(IN.worldPos.xyz))); // linear interpolation of interpolated vertex normal and approximated normal


			const float3 upVec = float3(0.0f, 1.0f, 0.0f);
			float dotResult = dot(normalize(IN.worldNormal), upVec);


			if (acos(dotResult) < limitAngleDirtTexture) // between angles 0 to limitAngleDirtTexture interpolate between grass and dirt texture
			{
				weightGrass = 1.0f - acos(dotResult) / limitAngleDirtTexture;
				weightDirt = acos(dotResult) / limitAngleDirtTexture;
				weightStone = 0.0f;
			}
			else // between angles limitAngleDirtTexture to limitAngleStoneTexture interpolate between dirt and stone texture (limitAngleStoneTexture to 90 degrees -> also stone texture)
			{
				weightGrass = 0.0f;
				weightDirt = 1.0f - min(1.0f, (acos(dotResult) - limitAngleDirtTexture) / limitAngleStoneTexture);
				weightStone = min(1.0f, (acos(dotResult) - limitAngleDirtTexture) / limitAngleStoneTexture);
			}

			/*
			float dist = max(abs(IN.worldPos.x - _WorldSpaceCameraPos.x), abs(IN.worldPos.z - _WorldSpaceCameraPos.z));
			if (dist<_TerrainDistanceInWorldUnits)
			{
				weightGrass = 1.0f;
				weightDirt = 0.0f;
				weightStone = 0.0f;
			}
			*/

			if ((index==223)||(IN.worldPos.y < _WaterHeightTransformed)) // water (either by tile index or by tile world position)
			{
				c = getColorByTextureAtlasIndex(IN, _TileAtlasTexWoodland, 0);
			}
			else if ((index==224)||(index==225)||(index==229)) // desert
			{				
				c_g = getColorByTextureAtlasIndex(IN, _TileAtlasTexDesert, 8);
				c_d = getColorByTextureAtlasIndex(IN, _TileAtlasTexDesert, 4);
				c_s = getColorByTextureAtlasIndex(IN, _TileAtlasTexDesert, 12);
				c = c_g * weightGrass + c_d * weightDirt + c_s * weightStone;
			}
			else if ((index==227)||(index==228)) // swamp
			{
				c_g = getColorByTextureAtlasIndex(IN, _TileAtlasTexSwamp, 8);
				c_d = getColorByTextureAtlasIndex(IN, _TileAtlasTexSwamp, 4);
				c_s = getColorByTextureAtlasIndex(IN, _TileAtlasTexSwamp, 12);
				c = c_g * weightGrass + c_d * weightDirt + c_s * weightStone;
			}
			else if ((index==226)||(index==230)) // mountain
			{
				c_g = getColorByTextureAtlasIndex(IN, _TileAtlasTexMountain, 8);
				c_d = getColorByTextureAtlasIndex(IN, _TileAtlasTexMountain, 4);
				c_s = getColorByTextureAtlasIndex(IN, _TileAtlasTexMountain, 12);
				c = c_g * weightGrass + c_d * weightDirt + c_s * weightStone;
			}
			else if ((index==231)||(index==232)||(index==233)) // moderate
			{
				c_g = getColorByTextureAtlasIndex(IN, _TileAtlasTexWoodland, 8);
				c_d = getColorByTextureAtlasIndex(IN, _TileAtlasTexWoodland, 4);
				c_s = getColorByTextureAtlasIndex(IN, _TileAtlasTexWoodland, 12);
				c = c_g * weightGrass + c_d * weightDirt + c_s * weightStone;
			}
			else
			{
				c=half4(0.0f, 0.0f, 0.0f, 1.0f);
			}
		
			o.Albedo = c.rgb;
			o.Alpha = c.a;
			
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
