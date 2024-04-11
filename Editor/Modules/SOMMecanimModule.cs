using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.Animations;
using AnimatorController = UnityEditor.Animations.AnimatorController;

namespace SOM{
	/// <summary>
	/// The Mecanim Module gets every Animator Controller in the project and adds info about its name, properties, layers, states and sub-states
	/// </summary>
	public class SOMMecanimModule:SOMModule{
		//===================================
		//Consts
		//===================================
		const string ADD_CONTROLLER_NAME_KEY = "Mecanim Module Add Controller Name Key";
		const string ADD_PARAMETERS_KEY = "Mecanim Module Add Parameters";
		const string ADD_LAYERS_KEY = "Mecanim Module Add Layers";
		const string ADD_LAYER_NAME_KEY = "Mecanim Module Add Layer Name Key";
		const string ADD_STATES_KEY = "Mecanim Module Add States Key";
		const string ADD_STATE_MACHINES_KEY = "Mecanim Module Add State Machines Key";
		const string ADD_STATE_MACHINES_NAME_KEY = "Mecanim Module Add State Machines Name Key";

		const string ADD_CONTROLLER_NAME_LABEL = "Add Controller's Name";
		const string ADD_PARAMETERS_LABEL = "Add Parameters";
		const string ADD_LAYERS_LABEL = "Add Layers";
		const string ADD_LAYER_NAME_LABEL = "Add Layers's Name";
		const string ADD_STATES_LABEL = "Add States";
		const string ADD_STATE_MACHINES_LABEL = "Add State Machine";
		const string ADD_STATE_MACHINES_NAME_LABEL = "Add State Machine Name";

		const string ADD_CONTROLLER_NAME_TOOLTIP = "Add the name of the controller as a constant to that controller's module";
		const string ADD_PARAMETERS_TOOLTIP = "For every controller, whether or not to a add a submodule with a list of its parameters";
		const string ADD_LAYERS_TOOLTIP = "For every controller, should a submodule containing a list of that controller's layers be added?";
		const string ADD_LAYER_NAME_TOOLTIP = "Add the name of the layer as a constant to that layer's module";
		const string ADD_STATES_TOOLTIP = "Should a submodule containing a list of state names be added?";
		const string ADD_STATE_MACHINES_TOOLTIP= "Should state machines inside every state machine be added recursively?";
		const string ADD_STATE_MACHINES_NAME_TOOLTIP= "Should the name of the sub state machine be added as a constant?";

		//===================================
		//Properties
		//===================================
		public override string moduleName{
			get{return "Mecanim";}
		}
		static bool addControllerName{
			get{
				if (!SOMPreferences.bools.Contains(ADD_CONTROLLER_NAME_KEY))
					addControllerName = true;
				return SOMPreferences.bools[ADD_CONTROLLER_NAME_KEY];
			}
			set{
				SOMPreferences.bools[ADD_CONTROLLER_NAME_KEY] = value;
			}
		}
		static bool addParameters{
			get{
				if (!SOMPreferences.bools.Contains(ADD_PARAMETERS_KEY))
					addParameters = true;
				return SOMPreferences.bools[ADD_PARAMETERS_KEY];
			}
			set{
				SOMPreferences.bools[ADD_PARAMETERS_KEY] = value;
			}
		}
		static bool addLayers{
			get{
				if (!SOMPreferences.bools.Contains(ADD_LAYERS_KEY))
					addLayers = true;
				return SOMPreferences.bools[ADD_LAYERS_KEY];
			}
			set{
				SOMPreferences.bools[ADD_LAYERS_KEY] = value;
			}
		}
		static bool addLayerName{
			get{
				if (!SOMPreferences.bools.Contains(ADD_LAYER_NAME_KEY))
					addLayerName = true;
				return SOMPreferences.bools[ADD_LAYER_NAME_KEY];
			}
			set{
				SOMPreferences.bools[ADD_LAYER_NAME_KEY] = value;
			}
		}
		static bool addStates{
			get{
				if (!SOMPreferences.bools.Contains(ADD_STATES_KEY))
					addStates = true;
				return SOMPreferences.bools[ADD_STATES_KEY];
			}
			set{
				SOMPreferences.bools[ADD_STATES_KEY] = value;
			}
		}
		static bool addStateMachines{
			get{
				if (!SOMPreferences.bools.Contains(ADD_STATE_MACHINES_KEY))
					addStateMachines = true;
				return SOMPreferences.bools[ADD_STATE_MACHINES_KEY];
			}
			set{
				SOMPreferences.bools[ADD_STATE_MACHINES_KEY] = value;
			}
		}
		static bool addStateMachineName{
			get{
				if (!SOMPreferences.bools.Contains(ADD_STATE_MACHINES_NAME_KEY))
					addStateMachineName = true;
				return SOMPreferences.bools[ADD_STATE_MACHINES_NAME_KEY];
			}
			set{
				SOMPreferences.bools[ADD_STATE_MACHINES_NAME_KEY] = value;
			}
		}

		//===================================
		//Refresh
		//===================================
		public override void Refresh(){
			//Get a list of AnimatorControllers
			string[] guids = AssetDatabase.FindAssets("t:AnimatorController");
			List<AnimatorController> controllers = new List<AnimatorController>();
			for (int i = 0; i < guids.Length; i++){
				string path = AssetDatabase.GUIDToAssetPath(guids[i]);
				//Exclude those in an editor folder
				if (path.IndexOf("/editor/",System.StringComparison.InvariantCultureIgnoreCase) != -1)
					continue;
				controllers.Add((AnimatorController)AssetDatabase.LoadAssetAtPath(path, typeof(AnimatorController)));
			}

			string subModule = moduleName+".controllers";
			SOMXmlHandler.AddModule(subModule);

			//Foreach controller...
			for (int i = 0; i < controllers.Count; i++){
				string controllerModule = subModule+"."+controllers[i].name;
				//Check if it's name isn't duplicated
				try{
					SOMXmlHandler.AddModule(controllerModule);
				}
				catch(ModuleAlreadyExistsException){
					SOMUtils.LogWarning("The controller {0} at {1} was skipped due to a duplicated name. Try renaming it to something else", controllers[i].name, AssetDatabase.GetAssetPath(controllers[i]));
					continue;
				}

				//Add a constant with it's name
				if (addControllerName)
					SOMXmlHandler.AddConstant(controllerModule,"name",controllers[i].name);
				
				//Add a list of parameters for every controller
				if (addParameters){
					string parametersModule = controllerModule+".parameters";
					SOMXmlHandler.AddModule(parametersModule);
					for (int j = 0; j < controllers[i].parameters.Length; j++)
						SOMXmlHandler.AddConstant(parametersModule, SOMUtils.NicifyConstantName(controllers[i].parameters[j].name), controllers[i].parameters[j].name);
				}
					
				//Add layers module
				if (addLayers){
					string layersModule = controllerModule+".layers";
					SOMXmlHandler.AddModule(layersModule);
					//And for each layer
					for (int j = 0; j < controllers[i].layers.Length; j++){
						string layerModule = layersModule+"."+controllers[i].layers[j].name;
						//Add a layer with its name
						SOMXmlHandler.AddModule(layerModule);
						//And a constant too
						if (addLayerName)
							SOMXmlHandler.AddConstant(layerModule,"name",controllers[i].layers[j].name);

						//And add all of this layer's state machine states
						AddStateMachineRecursive(controllers[i].layers[j].stateMachine,layerModule);
					}
				}

				//Unload this controller from memory
				Resources.UnloadAsset(controllers[i]);
			}
		}

		//Given a state machine, this module adds all of its states and sub state machines
		void AddStateMachineRecursive(AnimatorStateMachine stateMachine, string ownerModule){
			//Add a constant for every state
			if (addStates){
				if (stateMachine.states.Length>0){
					string statesModule = ownerModule+".states";
					SOMXmlHandler.AddModule(statesModule);
					for (int i = 0; i < stateMachine.states.Length; i++)
						SOMXmlHandler.AddConstant(statesModule, SOMUtils.NicifyConstantName(stateMachine.states[i].state.name),stateMachine.states[i].state.name);
				}
			}

			//If this state machine contains any sub state machine...
			if (addStateMachines){
				if (stateMachine.stateMachines.Length>0){
					string subStateMachinesModule = ownerModule+".State Machines";
					SOMXmlHandler.AddModule(subStateMachinesModule);
					//add a module of their own with...
					for (int i = 0; i < stateMachine.stateMachines.Length; i++){
						string stateMachineName = stateMachine.stateMachines[i].stateMachine.name;
						string stateMachineModule = subStateMachinesModule+"."+stateMachineName;
						SOMXmlHandler.AddModule(stateMachineModule);
						//Its name...
						if (addStateMachineName)
							SOMXmlHandler.AddConstant(stateMachineModule,"name", stateMachineName);
						//And all of its states
						AddStateMachineRecursive(stateMachine.stateMachines[i].stateMachine,stateMachineModule);
					}
				}
			}
		}

		//===================================
		//Repaint
		//===================================
		public override void DrawPreferences(){
			addControllerName = EditorGUILayout.ToggleLeft(new GUIContent(ADD_CONTROLLER_NAME_LABEL, ADD_CONTROLLER_NAME_TOOLTIP),addControllerName);
			addParameters = EditorGUILayout.ToggleLeft(new GUIContent(ADD_PARAMETERS_LABEL, ADD_PARAMETERS_TOOLTIP),addParameters);
			addLayers = EditorGUILayout.ToggleLeft(new GUIContent(ADD_LAYERS_LABEL, ADD_LAYERS_TOOLTIP),addLayers);
			GUI.enabled = addLayers;
			//Only if addLayers is active
			addLayerName = EditorGUILayout.ToggleLeft(new GUIContent(ADD_LAYER_NAME_LABEL, ADD_LAYER_NAME_TOOLTIP), addLayerName);
			addStates = EditorGUILayout.ToggleLeft(new GUIContent(ADD_STATES_LABEL, ADD_STATES_TOOLTIP), addStates);
			addStateMachines = EditorGUILayout.ToggleLeft(new GUIContent(ADD_STATE_MACHINES_LABEL, ADD_STATE_MACHINES_TOOLTIP), addStateMachines);
			GUI.enabled = addLayers && addStateMachines;
			//Only active when state machines are being added
			addStateMachineName = EditorGUILayout.ToggleLeft(new GUIContent(ADD_STATE_MACHINES_NAME_LABEL, ADD_STATE_MACHINES_NAME_TOOLTIP), addStateMachineName);
			GUI.enabled = true;
			base.DrawPreferences();
		}
	}
}