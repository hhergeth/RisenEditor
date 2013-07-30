using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using GameLibrary.IO;
using System.ComponentModel;
using SlimDX;
using GameLibrary;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Windows.Forms.Design;
using System.Windows.Forms;
using System.Collections;

namespace RisenEditor.Code.RisenTypes
{
    public class gCCombatSpecies : classData
    {
        short Version;
        List<bCAccessorPropertyObject> data0, data1, data2;

        public override void deSerialize(IFile a_File)
        {
            Version = a_File.Read<short>();
            data0 = new List<bCAccessorPropertyObject>(a_File.Read<int>());
            for (int i = 0; i < data0.Capacity; i++)
                data0.Add(new bCAccessorPropertyObject(a_File));
            data1 = new List<bCAccessorPropertyObject>(a_File.Read<int>());
            for (int i = 0; i < data1.Capacity; i++)
                data1.Add(new bCAccessorPropertyObject(a_File));
            data2 = new List<bCAccessorPropertyObject>(a_File.Read<int>());
            for (int i = 0; i < data2.Capacity; i++)
                data2.Add(new bCAccessorPropertyObject(a_File));

        }

        public override void Serialize(IFile a_File)
        {
            a_File.Write<short>(Version);
            a_File.Write<int>(data0.Count);
            foreach (bCAccessorPropertyObject b in data0)
                b.Serialize(a_File);
            a_File.Write<int>(data1.Count);
            foreach (bCAccessorPropertyObject b in data1)
                b.Serialize(a_File);
            a_File.Write<int>(data2.Count);
            foreach (bCAccessorPropertyObject b in data2)
                b.Serialize(a_File);
        }

        public override int Size
        {
            get { return 2 + data0.SizeOf() + data1.SizeOf() + data2.SizeOf(); }
        }
    }

    public class gCCombatStyleWeaponConfig : bCObjectBase
    {
        public gCCombatStyleWeaponConfig(IFile a_File)
        {
            deSerialize(a_File);
        }

        public gCCombatStyleWeaponConfig()
        {

        }
    }

    public class gCCombatStyleAniPose : bCObjectBase
    {
        public gCCombatStyleAniPose(IFile a_File)
        {
            deSerialize(a_File);
        }

        public gCCombatStyleAniPose()
        {

        }
    }

    public class gCCombatStyleMelee : bCObjectRefBase
    {
        public gCCombatStyleMelee(IFile a_File)
        {
            deSerialize(a_File);
        }

        public gCCombatStyleMelee()
        {

        }
    }

    public class gCCombatStyleRanged : bCObjectRefBase
    {
        public gCCombatStyleRanged(IFile a_File)
        {
            deSerialize(a_File);
        }

        public gCCombatStyleRanged()
        {

        }
    }

    public class gCCombatMoveMeleePhase : bCObjectBase
    {
        public gCCombatMoveMeleePhase(IFile a_File)
        {
            deSerialize(a_File);
        }

        public gCCombatMoveMeleePhase()
        {

        }
    }

    public class gCCombatMoveMelee : bCObjectRefBase
    {
        public gCCombatMoveMelee(IFile a_File)
        {
            deSerialize(a_File);
        }

        public gCCombatMoveMelee()
        {

        }
    }

    public class gCCombatMoveScriptState : bCObjectRefBase
    {
        public gCCombatMoveScriptState(IFile a_File)
        {
            deSerialize(a_File);
        }

        public gCCombatMoveScriptState()
        {

        }
    }

    public class gCCombatMoveStumble : bCObjectRefBase
    {
        public gCCombatMoveStumble(IFile a_File)
        {
            deSerialize(a_File);
        }

        public gCCombatMoveStumble()
        {

        }
    }

    public class gCCombatMoveOverlayStumble : bCObjectRefBase
    {
        public gCCombatMoveOverlayStumble(IFile a_File)
        {
            deSerialize(a_File);
        }

        public gCCombatMoveOverlayStumble()
        {

        }
    }

    public class gCCombatMoveFinishing : bCObjectRefBase
    {
        public gCCombatMoveFinishing(IFile a_File)
        {
            deSerialize(a_File);
        }

        public gCCombatMoveFinishing()
        {

        }
    }

    public class gCCombatMoveParade : bCObjectRefBase
    {
        public gCCombatMoveParade(IFile a_File)
        {
            deSerialize(a_File);
        }

        public gCCombatMoveParade()
        {

        }
    }

    public class gCCombatMoveReload : bCObjectRefBase
    {
        public gCCombatMoveReload(IFile a_File)
        {
            deSerialize(a_File);
        }

        public gCCombatMoveReload()
        {

        }
    }

    public class gCCombatMoveShoot : bCObjectRefBase
    {
        public gCCombatMoveShoot(IFile a_File)
        {
            deSerialize(a_File);
        }

        public gCCombatMoveShoot()
        {

        }
    }

    public class gCCombatMoveAim : bCObjectRefBase
    {
        public gCCombatMoveAim(IFile a_File)
        {
            deSerialize(a_File);
        }

        public gCCombatMoveAim()
        {

        }
    }

    public class gCCombatAIMelee : bCObjectRefBase
    {
        public gCCombatAIMelee(IFile a_File)
        {
            deSerialize(a_File);
        }

        public gCCombatAIMelee()
        {

        }
    }

    public class gCCombatAIRanged : bCObjectRefBase
    {
        public gCCombatAIRanged(IFile a_File)
        {
            deSerialize(a_File);
        }

        public gCCombatAIRanged()
        {

        }
    }
}
