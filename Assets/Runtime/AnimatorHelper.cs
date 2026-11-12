#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor.Animations;
using UnityEngine;

using static VRC.SDK3.Avatars.Components.VRCAvatarDescriptor;

/*
 * VRSuya Core
 * Contact : vrsuya@gmail.com // Twitter : https://twitter.com/VRSuya
 */

namespace VRSuya.Core {

	public static class AnimatorHelper {

		static void CollectAnimatorComponent<TargetComponent>(AnimatorStateMachine TargetStateMachine, List<TargetComponent> TargetComponents) where TargetComponent : StateMachineBehaviour {
			foreach (StateMachineBehaviour TargetBehaviour in TargetStateMachine.behaviours) {
				if (TargetBehaviour is TargetComponent TargetStateMachineBehaviour) {
					TargetComponents.Add(TargetStateMachineBehaviour);
				}
			}
			foreach (ChildAnimatorState ChildState in TargetStateMachine.states) {
				AnimatorState TargetState = ChildState.state;
				if (!TargetState) continue;
				foreach (StateMachineBehaviour TargetBehaviour in TargetState.behaviours) {
					if (TargetBehaviour is TargetComponent TargetStateMachineBehaviourl) {
						TargetComponents.Add(TargetStateMachineBehaviourl);
					}
				}
			}
			foreach (ChildAnimatorStateMachine ChildStateMachine in TargetStateMachine.stateMachines) {
				AnimatorStateMachine ChildSubStateMachine = ChildStateMachine.stateMachine;
				if (!ChildSubStateMachine) continue;
				CollectAnimatorComponent(ChildSubStateMachine, TargetComponents);
			}
		}

		public static AnimationClip[] GetAllAnimationClips(AnimatorController TargetAnimator) {
			if (!TargetAnimator) return new AnimationClip[0];
			List<AnimationClip> AnimatorAnimationClips = new List<AnimationClip>();
			List<Motion> AnimatorMotionList = GetAllAnimatorStates(TargetAnimator)
				.Where(Item => Item.motion != null)
				.Select(Item => Item.motion)
				.ToList();
			List<BlendTree> ChildBlendTrees = AnimatorMotionList.Where(Item => Item is BlendTree).Select(Item => Item as BlendTree).ToList();
			AnimatorAnimationClips.AddRange(AnimatorMotionList.Where(Item => Item is AnimationClip).Select(Item => Item as AnimationClip));
			foreach (BlendTree TargetChildBlendTree in ChildBlendTrees) {
				AnimatorAnimationClips.AddRange(GetAllAnimationClipsFromBlendTree(TargetChildBlendTree));
			}
			return AnimatorAnimationClips.Distinct().ToArray();
		}

		public static AnimationClip[] GetAllAnimationClipsFromBlendTree(BlendTree TargetBlendTree) {
			if (!TargetBlendTree) return new AnimationClip[0];
			List<AnimationClip> BlendTreeAnimationClips = new List<AnimationClip>();
			List<Motion> BlendTreeMotions = TargetBlendTree.children.Where(Item => Item.motion != null).Select(Item => Item.motion).ToList();
			List<BlendTree> ChildBlendTrees = BlendTreeMotions.Where(Item => Item is BlendTree).Select(Item => Item as BlendTree).ToList();
			BlendTreeAnimationClips.AddRange(BlendTreeMotions.Where(Item => Item is AnimationClip).Select(Item => Item as AnimationClip));
			foreach (BlendTree TargetChildBlendTree in ChildBlendTrees) {
				BlendTreeAnimationClips.AddRange(GetAllAnimationClipsFromBlendTree(TargetChildBlendTree));
			}
			return BlendTreeAnimationClips.Distinct().ToArray();
		}

		public static AnimatorState[] GetAllAnimatorStates(AnimatorController TargetAnimatorController) {
			List<AnimatorStateMachine> AllAnimatorStateMachine = new List<AnimatorStateMachine>();
			List<AnimatorState> AllAnimatorState = new List<AnimatorState>();
			foreach (AnimatorControllerLayer TargetAnimatorLayer in TargetAnimatorController.layers) {
				AllAnimatorStateMachine.AddRange(GetAllStateMachines(TargetAnimatorLayer.stateMachine).ToList());
			}
			foreach (AnimatorStateMachine TargetAnimatorStateMachine in AllAnimatorStateMachine) {
				AllAnimatorState.AddRange(GetAllStates(TargetAnimatorStateMachine).ToList());
			}
			return AllAnimatorState.ToArray();
		}

		public static AnimationClip[] GetAllAvatarAnimationClips(GameObject AvatarGameObject) {
			List<AnimatorController> AvatarAnimatorControllers = new List<AnimatorController>();
			List<AnimationClip> AvatarAnimationClips = new List<AnimationClip>();
			AvatarAnimatorControllers.Add(AvatarUtility.GetAnimatorController(AvatarGameObject, AnimLayerType.Action));
			AvatarAnimatorControllers.Add(AvatarUtility.GetAnimatorController(AvatarGameObject, AnimLayerType.Additive));
			AvatarAnimatorControllers.Add(AvatarUtility.GetAnimatorController(AvatarGameObject, AnimLayerType.Base));
			AvatarAnimatorControllers.Add(AvatarUtility.GetAnimatorController(AvatarGameObject, AnimLayerType.FX));
			AvatarAnimatorControllers.Add(AvatarUtility.GetAnimatorController(AvatarGameObject, AnimLayerType.Gesture));
			AvatarAnimatorControllers.Add(AvatarUtility.GetAnimatorController(AvatarGameObject, AnimLayerType.IKPose));
			AvatarAnimatorControllers.Add(AvatarUtility.GetAnimatorController(AvatarGameObject, AnimLayerType.Sitting));
			AvatarAnimatorControllers.Add(AvatarUtility.GetAnimatorController(AvatarGameObject, AnimLayerType.TPose));
			foreach (AnimatorController TargetAnimator in AvatarAnimatorControllers) {
				AvatarAnimationClips.AddRange(GetAllAnimationClips(TargetAnimator));
			}
			return AvatarAnimationClips.Distinct().ToArray();
		}

		public static AnimatorStateMachine[] GetAllStateMachines(AnimatorStateMachine TargetStateMachine) {
			AnimatorStateMachine[] StateMachines = new AnimatorStateMachine[] { TargetStateMachine };
			if (TargetStateMachine.stateMachines.Length > 0) {
				foreach (var TargetChildStateMachine in TargetStateMachine.stateMachines) {
					StateMachines = StateMachines.Concat(GetAllStateMachines(TargetChildStateMachine.stateMachine)).ToArray();
				}
			}
			return StateMachines;
		}

		public static AnimatorState[] GetAllStates(AnimatorStateMachine TargetStateMachine) {
			AnimatorState[] States = TargetStateMachine.states.Select(ExistChildState => ExistChildState.state).ToArray();
			if (TargetStateMachine.stateMachines.Length > 0) {
				foreach (var TargetChildStatetMachine in TargetStateMachine.stateMachines) {
					States = States.Concat(GetAllStates(TargetChildStatetMachine.stateMachine)).ToArray();
				}
			}
			return States;
		}

		public static TargetComponent[] GetAnimatorComponent<TargetComponent>(AnimatorStateMachine TargetStateMachine) where TargetComponent : StateMachineBehaviour {
			List<TargetComponent> TargetComponents = new List<TargetComponent>();
			CollectAnimatorComponent(TargetStateMachine, TargetComponents);
			return TargetComponents.ToArray();
		}

		public static AnimatorState GetStandingState(AnimatorController TargetAnimator) {
			if (TargetAnimator.layers.Length > 0) {
				AnimatorState[] AllAnimatorStates = GetAllStates(TargetAnimator.layers[0].stateMachine);
				AnimatorState StandingState = AllAnimatorStates.FirstOrDefault(Item => Item.name == "Standing");
				if (StandingState) return StandingState;
				StandingState = AllAnimatorStates.FirstOrDefault(Item => Item.name.Contains("Stand", StringComparison.OrdinalIgnoreCase));
				if (StandingState) return StandingState;
				if (TargetAnimator.layers[0].stateMachine.defaultState) {
					return TargetAnimator.layers[0].stateMachine.defaultState;
				}
			}
			return null;
		}

		static BlendTree[] GetSubBlendTrees(BlendTree TargetBlendTree) {
			BlendTree[] BlendTrees = new BlendTree[0];
			BlendTrees = BlendTrees.Concat(new BlendTree[] { TargetBlendTree }).ToArray();
			foreach (ChildMotion ChildMotion in TargetBlendTree.children) {
				if (ChildMotion.motion is BlendTree ChildBlendTree) {
					BlendTrees = BlendTrees.Concat(GetSubBlendTrees(ChildBlendTree)).ToArray();
				}
			}
			return BlendTrees;
		}

		public static bool IsAnimatorWriteDefaults(AnimatorController TargetAnimator) {
			AnimatorState[] AvatarAnimatorState = GetAllAnimatorStates(TargetAnimator);
			bool[] WriteDefaults = AvatarAnimatorState.Select(Item => Item.writeDefaultValues).ToArray();
			int WriteDefaultsOffCount = WriteDefaults.Where(Item => Item == false).Count();
			if (WriteDefaults.Length > 0) {
				if ((float)WriteDefaultsOffCount / WriteDefaults.Length <= 0.5) {
					return true;
				} else {
					return false;
				}
			}
			return true;
		}
	}
}
#endif