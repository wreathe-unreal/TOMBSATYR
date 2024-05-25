using UnityEngine;
using System.Collections;
using Digicrafts.Animation;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Digicrafts.Gem {

	public class Gem : MonoBehaviour {

		public Color color = Color.white;
		public float opacity = 0.9f;
		public float reflection = 0.6f;
		public float refraction = 0.6f;
		public float lighting = 0.6f;
		[MinMaxSlider (0f, 3f)]
		public Vector2 glowMinMax = new Vector2(0.5f,2);
		public float glowAnimationTime = 1;
		public float rotateAnimationTime = 5;
		public float floatAnimationTime = 2;
		public float floatAnimationHeight = 0.2f;

		// Var for animation
		private float _rotationStep = 0;
		private float _glowAnimationStep = 0;
		private float _glowDirection = 1;
		private float _floatAnimationStep = 0;
		private float _floatDirection = 1;
		private float _originY = 0;

		// Use this for initialization
		void Start () {								
			_originY = gameObject.transform.localPosition.y;
		}

		// Update is called once per frame
		void Update () {

			// rotate animation
			if(rotateAnimationTime>0){			
				_rotationStep =  Time.deltaTime/rotateAnimationTime*360;
				gameObject.transform.RotateAround(gameObject.transform.position,Vector3.up,_rotationStep);
			}

			// glow animation
			if(glowAnimationTime>0&&glowMinMax.x!=glowMinMax.y){
				float glow;
				float step = _glowAnimationStep/glowAnimationTime;
				if(_glowDirection==1){
					glow = (glowMinMax.y - glowMinMax.x) * Easing.EaseOut(step,EasingType.Sine) + glowMinMax.x;
				} else {
					glow = (glowMinMax.y - glowMinMax.x) * (1-step) + glowMinMax.x;
				}
				gameObject.GetComponent<MeshRenderer>().material.SetFloat("_Emission", glow);

				//
				_glowAnimationStep+=Time.deltaTime;
				// Set glow step
				if(_glowAnimationStep>glowAnimationTime){
					_glowAnimationStep=0;
					_glowDirection=-_glowDirection;
				}
			}

			// glow animation
			if(floatAnimationTime>0){
				float f;
				float step = _floatAnimationStep/floatAnimationTime;
				if(_floatDirection==1){
					f = floatAnimationHeight * Easing.EaseInOut(step,EasingType.Sine) + _originY;
				} else {
					f = floatAnimationHeight * Easing.EaseInOut(1-step,EasingType.Sine) + _originY;
				}
				gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x,f,gameObject.transform.localPosition.z);

				//
				_floatAnimationStep+=Time.deltaTime;
				// Set glow step
				if(_floatAnimationStep>floatAnimationTime){
					_floatAnimationStep=0;
					_floatDirection=-_floatDirection;
				}
			}
				
		}

		public void OnValidate(){
			
			#if UNITY_EDITOR
//			Debug.Log("OnValidate: "+ sharedMaterial + " shared: " + gameObject.GetComponent<MeshRenderer>().sharedMaterial);

			if(Application.isEditor&&Application.isPlaying){
				gameObject.GetComponent<MeshRenderer>().material.color=color;
				gameObject.GetComponent<MeshRenderer>().material.SetFloat("_Opacity", opacity);
				gameObject.GetComponent<MeshRenderer>().material.SetFloat("_RefractionStrength", refraction);
				gameObject.GetComponent<MeshRenderer>().material.SetFloat("_ReflectionStrength", reflection);
				gameObject.GetComponent<MeshRenderer>().material.SetFloat("_EnvironmentLight", lighting);
			} else {
				
				if(Selection.activeGameObject&&PrefabUtility.GetPrefabType(Selection.activeGameObject)==PrefabType.PrefabInstance){
					foreach(GameObject obj in Selection.gameObjects){
						Material mat = obj.GetComponent<MeshRenderer>().sharedMaterial;
						mat.color=color;
						mat.SetFloat("_Opacity", opacity);
						mat.SetFloat("_RefractionStrength", refraction);
						mat.SetFloat("_ReflectionStrength", reflection);
						mat.SetFloat("_EnvironmentLight", lighting);
						obj.GetComponent<MeshRenderer>().sharedMaterial = new Material(mat);
					}
				}
			}

			#endif
		}

		// Setter

		public void SetColor(Color value) {			
			color=value;
			gameObject.GetComponent<MeshRenderer>().material.color=color;
		}

		public void SetOpacity(float value) {
			if(value>=0 && value<=1){
				opacity=value;
				gameObject.GetComponent<MeshRenderer>().material.SetFloat("_Opacity", opacity);
			}
		}

		public void SetReflection(float value) {
			if(value>=0 && value<=1){
				reflection=value;
				gameObject.GetComponent<MeshRenderer>().material.SetFloat("_ReflectionStrength", reflection);
			}
		}

		public void SetRefraction(float value) {
			if(value>=0 && value<=1){
				refraction=value;
				gameObject.GetComponent<MeshRenderer>().material.SetFloat("_RefractionStrength", refraction);
			}
		}

		public void SetLighting(float value) {
			if(value>=0 && value<=2){
				lighting=value;
				gameObject.GetComponent<MeshRenderer>().material.SetFloat("_EnvironmentLight", lighting);
			}
		}			
	}
}