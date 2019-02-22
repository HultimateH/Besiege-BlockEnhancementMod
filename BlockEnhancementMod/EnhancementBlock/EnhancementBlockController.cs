﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Modding;
using Modding.Blocks;
using BlockEnhancementMod.Blocks;
using System.Collections;

namespace BlockEnhancementMod
{
    class EnhancementBlockController : SingleInstance<EnhancementBlockController>
    {
        public override string Name { get; } = "Enhancement Block Controller";

        [Obsolete]
        /// <summary>存档信息</summary>
        internal PlayerMachineInfo PMI;

        //public event Action<PlayerMachineInfo> OnLoad;
        //public event Action<PlayerMachineInfo> OnSave;

        private void Awake()
        {
            ////加载配置
            //Events.OnMachineLoaded += LoadConfiguration;
            ////储存配置
            //Events.OnMachineSave += SaveConfiguration;
            //添加零件初始化事件委托
            Events.OnBlockInit += AddSliders;
        }

        /// <summary>是否有进阶属性</summary>
        public static bool HasEnhancement(BlockBehaviour block)
        {
            return block.MapperTypes.Exists(match => match.Key == "Enhancement");
        }

        /// <summary>所有零件添加进阶属性控件</summary>
        public void AddAllSliders()
        {
            foreach (BlockBehaviour block in Machine.Active().BuildingBlocks.FindAll(block => !HasEnhancement(block)))
            {
                AddSliders(block);
            }
        }

        /// <summary>零件添加进阶属性控件 </summary>
        private void AddSliders(Block block)
        {
#if DEBUG
            ConsoleController.ShowMessage("on block init");
#endif
            BlockBehaviour blockbehaviour = block.BuildingBlock.InternalObject;
            if (!HasEnhancement(blockbehaviour))
                AddSliders(blockbehaviour);
        }
        /// <summary>零件添加进阶属性控件 </summary>
        private void AddSliders(Transform block)
        {
            BlockBehaviour blockbehaviour = block.GetComponent<BlockBehaviour>();
            if (!HasEnhancement(blockbehaviour))
                AddSliders(blockbehaviour);
        }
        /// <summary>零件添加进阶属性控件 </summary>
        private void AddSliders(BlockBehaviour block)
        {
#if DEBUG
            ConsoleController.ShowMessage(string.Format("Block ID: {0}", block.BlockID.ToString()));
#endif

            if (dic_EnhancementBlock.ContainsKey(block.BlockID))
            {
                var EB = dic_EnhancementBlock[block.BlockID];

                if (block.GetComponent(EB) == null)
                {
                    block.gameObject.AddComponent(EB);
                }
            }
        }

        /// <summary>模块扩展脚本字典   通过字典自动为模块加载扩展脚本</summary>
        public Dictionary<int, Type> dic_EnhancementBlock = new Dictionary<int, Type>
        {
            {(int)BlockType.BallJoint,typeof(BallJointScript) },
            {(int)BlockType.Cannon,typeof(CannonScript) },
            ////{(int)BlockType.CogLargeUnpowered,typeof(cog) },
            ////{(int)BlockType.CogMediumPowered,typeof(CannonScript) },
            ////{(int)BlockType.CogMediumUnpowered,typeof(CannonScript) },
            {(int)BlockType.Decoupler,typeof(DecouplerScript) },
            {(int)BlockType.GripPad,typeof(GripPadScript) },
            {(int)BlockType.Piston,typeof(PistonScript) },
            {(int)BlockType.Propeller,typeof(PropellerScript) },
            {(int)BlockType.SmallPropeller,typeof(PropellerScript) },
            { (int)BlockType.Wing,typeof(PropellerScript)},
            { (int)BlockType.WingPanel,typeof(PropellerScript)},
            { (int)BlockType.Unused3,typeof(PropellerScript)},
            {(int)BlockType.Slider,typeof(SliderScript) },
            {(int)BlockType.SmallWheel,typeof(SmallwheelScript) },
            ////{(int)BlockType.SpinningBlock,typeof(SpinningScript) },
            {(int)BlockType.Spring,typeof(SpringScript) },
            {(int)BlockType.SteeringHinge,typeof(SteeringHinge) },
            {(int)BlockType.SteeringBlock,typeof(SteeringHinge) },
            {(int)BlockType.Suspension,typeof(SuspensionScript) },
            { (int)BlockType.Flamethrower,typeof(FlamethrowerScript)},
            {(int)BlockType.Wheel,typeof(WheelScript) },
            {(int)BlockType.LargeWheel,typeof(WheelScript) },
            {(int)BlockType.LargeWheelUnpowered,typeof(WheelScript) },
            {(int)BlockType.WheelUnpowered,typeof(WheelScript) },
            {(int)BlockType.Rocket,typeof(RocketScript)},
            {(int)BlockType.CameraBlock,typeof(CameraScript)},
            { (int)BlockType.SingleWoodenBlock,typeof(WoodenScript)},
            { (int)BlockType.DoubleWoodenBlock,typeof(WoodenScript)},
            { (int)BlockType.Log,typeof(WoodenScript)},
            { (int)BlockType.WoodenPanel,typeof(WoodenScript)},
            { (int)BlockType.WoodenPole,typeof(WoodenScript)},
            { (int)BlockType.WaterCannon,typeof(WaterCannonScript) },
        };

        [Obsolete]
        /// <summary>刷新菜单组件</summary>
        public IEnumerator RefreshSliders()
        {
            int i = 0;
            while (i++ < 3)
            {
                yield return new WaitForEndOfFrame();
            }
            foreach (BlockBehaviour block in Machine.Active().BuildingBlocks)
            {
                AddSliders(block);
            }

#if DEBUG
            ConsoleController.ShowMessage("Refresh");
#endif
        }     
    }
}
