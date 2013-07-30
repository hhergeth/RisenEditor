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
    public class gCInfo : classData
    {
        short Version;
        List<bCAccessorPropertyObject> m_pCommands;
        List<bCAccessorPropertyObject> m_pConditions;

        public override void deSerialize(IFile a_File)
        {
            Version = a_File.Read<short>();

            //gCInfo__ReadCompiledComSeq
            short s0 = a_File.Read<short>();
            int i0 = a_File.Read<int>();
            if (i0 > 0)
            {
                m_pCommands = new List<bCAccessorPropertyObject>();
                for (int i = 0; i < i0; i++)
                    m_pCommands.Add(new bCAccessorPropertyObject(a_File));
            }

            //gCInfo__ReadCompiledCondSeq
            short s1 = a_File.Read<short>();
            if (s1 == 1)
            {
                int i1 = a_File.Read<int>();
                if (i1 > 0)
                {
                    m_pConditions = new List<bCAccessorPropertyObject>();
                    for (int i = 0; i < i1; i++)
                        m_pConditions.Add(new bCAccessorPropertyObject(a_File));
                }
            }
        }

        public override void Serialize(IFile a_File)
        {
            a_File.Write<short>(Version);
            a_File.Write<short>(m_pCommands != null ? (short)1 : (short)0);
            a_File.Write<int>(m_pCommands != null ? m_pCommands.Count : 0);
            if (m_pCommands != null)
                foreach (bCAccessorPropertyObject a in m_pCommands)
                    a.Serialize(a_File);
            a_File.Write<short>(m_pConditions != null ? (short)1 : (short)0);
            if (m_pConditions != null)
            {
                a_File.Write<int>(m_pConditions.Count);
                foreach (bCAccessorPropertyObject a in m_pConditions)
                    a.Serialize(a_File);
            }
        }

        public override int Size
        {
            get 
            {
                return 10 + (m_pCommands != null ? m_pCommands.SizeOf() - 4 : 0) + (m_pConditions != null ? m_pConditions.SizeOf() : 0);
            }
        }

        public void AddCommand(bCAccessorPropertyObject A)
        {
            if (m_pCommands == null)
                m_pCommands = new List<bCAccessorPropertyObject>();
            m_pCommands.Add(A);
        }

        public void InsertCommand(bCAccessorPropertyObject A, int i)
        {
            if (m_pCommands == null)
                m_pCommands = new List<bCAccessorPropertyObject>();
            m_pCommands.Insert(i, A);
        }

        public void RemoveCommand(int index)
        {
            m_pCommands.RemoveAt(index);
        }

        public void AddCondition(bCAccessorPropertyObject A)
        {
            if (m_pConditions == null)
                m_pConditions = new List<bCAccessorPropertyObject>();
            m_pConditions.Add(A);
        }

        public void InsertCondition(bCAccessorPropertyObject A, int i)
        {
            if (m_pConditions == null)
                m_pConditions = new List<bCAccessorPropertyObject>();
            m_pConditions.Insert(i, A);
        }

        public void RemoveCondition(int index)
        {
            m_pConditions.RemoveAt(index);
        }

        public bCAccessorPropertyObject[] getCommands()
        {
            if (m_pCommands == null)
                return new bCAccessorPropertyObject[0];
            else return m_pCommands.ToArray();
        }

        public bCAccessorPropertyObject[] getConditions()
        {
            if (m_pConditions == null)
                return new bCAccessorPropertyObject[0];
            else return m_pConditions.ToArray();
        }
    }

    public class gCQuest : classData
    {
        short Version;
        int[] i0;

        public override void deSerialize(IFile a_File)
        {
            Version = a_File.Read<short>();
            if (Version == 2)
                i0 = a_File.Read<int>(4);
        }

        public override void Serialize(IFile a_File)
        {
            a_File.Write<short>(Version);
            if (Version == 2)
                a_File.Write<int>(i0);
        }

        public override int Size
        {
            get { return Version == 2 ? 18 : 2; }
        }
    }

    public class SLogEntry : classData
    {
        public override void deSerialize(IFile a_File)
        {
            throw new NotImplementedException();
        }

        public override void Serialize(IFile a_File)
        {
            throw new NotImplementedException();
        }

        public override int Size
        {
            get { throw new NotImplementedException(); }
        }
    }

    public class gSDeliveryEntity : classData
    {
        byte[] B;

        public override void deSerialize(IFile a_File)
        {
            B = a_File.Read<byte>(10);
        }

        public override void Serialize(IFile a_File)
        {
            a_File.Write<byte>(B);
        }

        public override int Size
        {
            get { return 10; }
        }
    }

    public class gCQuestActor : bCObjectBase
    {
        public gCQuestActor()
        {
            Properties.addProperty("Actor", new eCEntityStringProxy(null as string));
            Properties.addProperty("ActorType", gEQuestActor.gEQuestActor_Client);
        }

        public gCQuestActor(string actor, gEQuestActor type)
        {
            Actor = new eCEntityStringProxy(actor);
            ActorType = type;
        }

        public gCQuestActor(IFile a_File)
        {
            deSerialize(a_File);
        }

        public eCEntityStringProxy Actor
        {
            get
            {
                return Properties["Actor"].Object as eCEntityStringProxy;
            }
            set
            {
                Properties["Actor"].Object = value;
            }
        }

        public gEQuestActor ActorType
        {
            get
            {
                return (gEQuestActor)Properties["ActorType"].Object;
            }
            set
            {
                Properties["ActorType"].Object = value;
            }
        }
    }

#region PropertySets
    public class gCInfoCommandSay : classData
    {
        short Version;

        public override void deSerialize(IFile a_File)
        {
            Version = a_File.Read<short>();
        }

        public override void Serialize(IFile a_File)
        {
            a_File.Write<short>(Version);
        }

        public override int Size
        {
            get
            {
                return 2;
            }
        }
    }
    public class gCInfoCommandSetGameEvent : classData
    {
        short Version;

        public override void deSerialize(IFile a_File)
        {
            Version = a_File.Read<short>();
        }

        public override void Serialize(IFile a_File)
        {
            a_File.Write<short>(Version);
        }

        public override int Size
        {
            get
            {
                return 2;
            }
        }
    }
    public class gCInfoCommandRunScript : classData
    {
        short Version;

        public override void deSerialize(IFile a_File)
        {
            Version = a_File.Read<short>();
        }

        public override void Serialize(IFile a_File)
        {
            a_File.Write<short>(Version);
        }

        public override int Size
        {
            get
            {
                return 2;
            }
        }
    }
    public class gCInfoCommandEnd : classData
    {
        short Version;

        public override void deSerialize(IFile a_File)
        {
            Version = a_File.Read<short>();
        }

        public override void Serialize(IFile a_File)
        {
            a_File.Write<short>(Version);
        }

        public override int Size
        {
            get
            {
                return 2;
            }
        }
    }
    public class gCInfoCommandRunQuest : classData
    {
        short Version;

        public override void deSerialize(IFile a_File)
        {
            Version = a_File.Read<short>();
        }

        public override void Serialize(IFile a_File)
        {
            a_File.Write<short>(Version);
        }

        public override int Size
        {
            get
            {
                return 2;
            }
        }
    }
    public class gCInfoCommandAddLogText : classData
    {
        short Version;

        public override void deSerialize(IFile a_File)
        {
            Version = a_File.Read<short>();
        }

        public override void Serialize(IFile a_File)
        {
            a_File.Write<short>(Version);
        }

        public override int Size
        {
            get
            {
                return 2;
            }
        }
    }
    public class gCInfoCommandAddInfoSystemEndScript : classData
    {
        short Version;

        public override void deSerialize(IFile a_File)
        {
            Version = a_File.Read<short>();
        }

        public override void Serialize(IFile a_File)
        {
            a_File.Write<short>(Version);
        }

        public override int Size
        {
            get
            {
                return 2;
            }
        }
    }
    public class gCInfoConditionPlayerKnows : classData
    {
        short Version;

        public override void deSerialize(IFile a_File)
        {
            Version = a_File.Read<short>();
        }

        public override void Serialize(IFile a_File)
        {
            a_File.Write<short>(Version);
        }

        public override int Size
        {
            get
            {
                return 2;
            }
        }
    }
    public class gCInfoCommandDescription : classData
    {
        short Version;

        public override void deSerialize(IFile a_File)
        {
            Version = a_File.Read<short>();
        }

        public override void Serialize(IFile a_File)
        {
            a_File.Write<short>(Version);
        }

        public override int Size
        {
            get
            {
                return 2;
            }
        }
    }
    public class gCInfoCommandPickPocket : classData
    {
        short Version;

        public override void deSerialize(IFile a_File)
        {
            Version = a_File.Read<short>();
        }

        public override void Serialize(IFile a_File)
        {
            a_File.Write<short>(Version);
        }

        public override int Size
        {
            get
            {
                return 2;
            }
        }
    }
    public class gCInfoCommandTeleportNPC : classData
    {
        short Version;

        public override void deSerialize(IFile a_File)
        {
            Version = a_File.Read<short>();
        }

        public override void Serialize(IFile a_File)
        {
            a_File.Write<short>(Version);
        }

        public override int Size
        {
            get
            {
                return 2;
            }
        }
    }
    public class gCInfoCommandSetRoutine : classData
    {
        short Version;

        public override void deSerialize(IFile a_File)
        {
            Version = a_File.Read<short>();
        }

        public override void Serialize(IFile a_File)
        {
            a_File.Write<short>(Version);
        }

        public override int Size
        {
            get
            {
                return 2;
            }
        }
    }
    public class gCInfoConditionNPCStatus : classData
    {
        short Version;

        public override void deSerialize(IFile a_File)
        {
            Version = a_File.Read<short>();
        }

        public override void Serialize(IFile a_File)
        {
            a_File.Write<short>(Version);
        }

        public override int Size
        {
            get
            {
                return 2;
            }
        }
    }
    public class gCInfoConditionPlayerNotKnows : classData
    {
        short Version;

        public override void deSerialize(IFile a_File)
        {
            Version = a_File.Read<short>();
        }

        public override void Serialize(IFile a_File)
        {
            a_File.Write<short>(Version);
        }

        public override int Size
        {
            get
            {
                return 2;
            }
        }
    }
    public class gCInfoCommandGiveXP : classData
    {
        short Version;

        public override void deSerialize(IFile a_File)
        {
            Version = a_File.Read<short>();
        }

        public override void Serialize(IFile a_File)
        {
            a_File.Write<short>(Version);
        }

        public override int Size
        {
            get
            {
                return 2;
            }
        }
    }
    public class gCInfoCommandCreateItem : classData
    {
        short Version;

        public override void deSerialize(IFile a_File)
        {
            Version = a_File.Read<short>();
        }

        public override void Serialize(IFile a_File)
        {
            a_File.Write<short>(Version);
        }

        public override int Size
        {
            get
            {
                return 2;
            }
        }
    }
    public class gCInfoCommandAddNPCInfo : classData
    {
        short Version;

        public override void deSerialize(IFile a_File)
        {
            Version = a_File.Read<short>();
        }

        public override void Serialize(IFile a_File)
        {
            a_File.Write<short>(Version);
        }

        public override int Size
        {
            get
            {
                return 2;
            }
        }
    }
    public class gCInfoCommandSaySVM : classData
    {
        short Version;

        public override void deSerialize(IFile a_File)
        {
            Version = a_File.Read<short>();
        }

        public override void Serialize(IFile a_File)
        {
            a_File.Write<short>(Version);
        }

        public override int Size
        {
            get
            {
                return 2;
            }
        }
    }
    public class gCInfoCommandTeach : classData
    {
        short Version;

        public override void deSerialize(IFile a_File)
        {
            Version = a_File.Read<short>();
        }

        public override void Serialize(IFile a_File)
        {
            a_File.Write<short>(Version);
        }

        public override int Size
        {
            get
            {
                return 2;
            }
        }
    }
    public class gCInfoConditionTeachEnabled : classData
    {
        short Version;

        public override void deSerialize(IFile a_File)
        {
            Version = a_File.Read<short>();
        }

        public override void Serialize(IFile a_File)
        {
            a_File.Write<short>(Version);
        }

        public override int Size
        {
            get
            {
                return 2;
            }
        }
    }
    public class gCInfoConditionCanTeachSkill : classData
    {
        short Version;

        public override void deSerialize(IFile a_File)
        {
            Version = a_File.Read<short>();
        }

        public override void Serialize(IFile a_File)
        {
            a_File.Write<short>(Version);
        }

        public override int Size
        {
            get
            {
                return 2;
            }
        }
    }
    public class gCInfoConditionSkillValue : classData
    {
        short Version;

        public override void deSerialize(IFile a_File)
        {
            Version = a_File.Read<short>();
        }

        public override void Serialize(IFile a_File)
        {
            a_File.Write<short>(Version);
        }

        public override int Size
        {
            get
            {
                return 2;
            }
        }
    }
    public class gCInfoConditionQuestStatus : classData
    {
        short Version;

        public override void deSerialize(IFile a_File)
        {
            Version = a_File.Read<short>();
        }

        public override void Serialize(IFile a_File)
        {
            a_File.Write<short>(Version);
        }

        public override int Size
        {
            get
            {
                return 2;
            }
        }
    }
    public class gCInfoCommandGive : classData
    {
        short Version;

        public override void deSerialize(IFile a_File)
        {
            Version = a_File.Read<short>();
        }

        public override void Serialize(IFile a_File)
        {
            a_File.Write<short>(Version);
        }

        public override int Size
        {
            get
            {
                return 2;
            }
        }
    }
    public class gCInfoCommandRunAIScript : classData
    {
        short Version;

        public override void deSerialize(IFile a_File)
        {
            Version = a_File.Read<short>();
        }

        public override void Serialize(IFile a_File)
        {
            a_File.Write<short>(Version);
        }

        public override int Size
        {
            get
            {
                return 2;
            }
        }
    }
    public class gCInfoCommandSetGuardStatus : classData
    {
        short Version;

        public override void deSerialize(IFile a_File)
        {
            Version = a_File.Read<short>();
        }

        public override void Serialize(IFile a_File)
        {
            a_File.Write<short>(Version);
        }

        public override int Size
        {
            get
            {
                return 2;
            }
        }
    }
    public class gCInfoCommandSucceedQuest : classData
    {
        short Version;

        public override void deSerialize(IFile a_File)
        {
            Version = a_File.Read<short>();
        }

        public override void Serialize(IFile a_File)
        {
            a_File.Write<short>(Version);
        }

        public override int Size
        {
            get
            {
                return 2;
            }
        }
    }
    public class gCInfoCommandSayConfirm : classData
    {
        short Version;

        public override void deSerialize(IFile a_File)
        {
            Version = a_File.Read<short>();
        }

        public override void Serialize(IFile a_File)
        {
            a_File.Write<short>(Version);
        }

        public override int Size
        {
            get
            {
                return 2;
            }
        }
    }
    public class gCInfoCommandCloseQuest : classData
    {
        short Version;

        public override void deSerialize(IFile a_File)
        {
            Version = a_File.Read<short>();
        }

        public override void Serialize(IFile a_File)
        {
            a_File.Write<short>(Version);
        }

        public override int Size
        {
            get
            {
                return 2;
            }
        }
    }
    public class gCInfoCommandSetTradeEnabled : classData
    {
        short Version;

        public override void deSerialize(IFile a_File)
        {
            Version = a_File.Read<short>();
        }

        public override void Serialize(IFile a_File)
        {
            a_File.Write<short>(Version);
        }

        public override int Size
        {
            get
            {
                return 2;
            }
        }
    }
    public class gCInfoCommandSetPartyEnabled : classData
    {
        short Version;

        public override void deSerialize(IFile a_File)
        {
            Version = a_File.Read<short>();
        }

        public override void Serialize(IFile a_File)
        {
            a_File.Write<short>(Version);
        }

        public override int Size
        {
            get
            {
                return 2;
            }
        }
    }
    public class gCInfoCommandAddQuestActor : classData
    {
        short Version;

        public override void deSerialize(IFile a_File)
        {
            Version = a_File.Read<short>();
        }

        public override void Serialize(IFile a_File)
        {
            a_File.Write<short>(Version);
        }

        public override int Size
        {
            get
            {
                return 2;
            }
        }
    }
    public class gCInfoConditionItemAmount : classData
    {
        short Version;

        public override void deSerialize(IFile a_File)
        {
            Version = a_File.Read<short>();
        }

        public override void Serialize(IFile a_File)
        {
            a_File.Write<short>(Version);
        }

        public override int Size
        {
            get
            {
                return 2;
            }
        }
    }
    public class gCInfoCommandSleep : classData
    {
        short Version;

        public override void deSerialize(IFile a_File)
        {
            Version = a_File.Read<short>();
        }

        public override void Serialize(IFile a_File)
        {
            a_File.Write<short>(Version);
        }

        public override int Size
        {
            get
            {
                return 2;
            }
        }
    }
    public class gCInfoCommandSetTeachEnabled : classData
    {
        short Version;

        public override void deSerialize(IFile a_File)
        {
            Version = a_File.Read<short>();
        }

        public override void Serialize(IFile a_File)
        {
            a_File.Write<short>(Version);
        }

        public override int Size
        {
            get
            {
                return 2;
            }
        }
    }
    public class gCInfoCommandInfoWait : classData
    {
        short Version;

        public override void deSerialize(IFile a_File)
        {
            Version = a_File.Read<short>();
        }

        public override void Serialize(IFile a_File)
        {
            a_File.Write<short>(Version);
        }

        public override int Size
        {
            get
            {
                return 2;
            }
        }
    }
    public class gCInfoConditionScript : classData
    {
        short Version;

        public override void deSerialize(IFile a_File)
        {
            Version = a_File.Read<short>();
        }

        public override void Serialize(IFile a_File)
        {
            a_File.Write<short>(Version);
        }

        public override int Size
        {
            get
            {
                return 2;
            }
        }
    }
    public class gCInfoCommandRemoveNPCInfo : classData
    {
        short Version;

        public override void deSerialize(IFile a_File)
        {
            Version = a_File.Read<short>();
        }

        public override void Serialize(IFile a_File)
        {
            a_File.Write<short>(Version);
        }

        public override int Size
        {
            get
            {
                return 2;
            }
        }
    }
    public class gCInfoCommandRunInfo : classData
    {
        short Version;

        public override void deSerialize(IFile a_File)
        {
            Version = a_File.Read<short>();
        }

        public override void Serialize(IFile a_File)
        {
            a_File.Write<short>(Version);
        }

        public override int Size
        {
            get
            {
                return 2;
            }
        }
    }
    public class gCInfoCommandClearGameEvent : classData
    {
        short Version;

        public override void deSerialize(IFile a_File)
        {
            Version = a_File.Read<short>();
        }

        public override void Serialize(IFile a_File)
        {
            a_File.Write<short>(Version);
        }

        public override int Size
        {
            get
            {
                return 2;
            }
        }
    }
    public class gCInfoCommandAttack : classData
    {
        short Version;

        public override void deSerialize(IFile a_File)
        {
            Version = a_File.Read<short>();
        }

        public override void Serialize(IFile a_File)
        {
            a_File.Write<short>(Version);
        }

        public override int Size
        {
            get
            {
                return 2;
            }
        }
    }
    public class gCInfoCommandStartFixCamera : classData
    {
        short Version;

        public override void deSerialize(IFile a_File)
        {
            Version = a_File.Read<short>();
        }

        public override void Serialize(IFile a_File)
        {
            a_File.Write<short>(Version);
        }

        public override int Size
        {
            get
            {
                return 2;
            }
        }
    }
    public class gCInfoCommandRemoveQuestActor : classData
    {
        short Version;

        public override void deSerialize(IFile a_File)
        {
            Version = a_File.Read<short>();
        }

        public override void Serialize(IFile a_File)
        {
            a_File.Write<short>(Version);
        }

        public override int Size
        {
            get
            {
                return 2;
            }
        }
    }
    public class gCInfoConditionOwnerInArea : classData
    {
        short Version;

        public override void deSerialize(IFile a_File)
        {
            Version = a_File.Read<short>();
        }

        public override void Serialize(IFile a_File)
        {
            a_File.Write<short>(Version);
        }

        public override int Size
        {
            get
            {
                return 2;
            }
        }
    }
    public class gCInfoCommandWear : classData
    {
        short Version;

        public override void deSerialize(IFile a_File)
        {
            Version = a_File.Read<short>();
        }

        public override void Serialize(IFile a_File)
        {
            a_File.Write<short>(Version);
        }

        public override int Size
        {
            get
            {
                return 2;
            }
        }
    }
    public class gCInfoConditionOwnerNearEntity : classData
    {
        short Version;

        public override void deSerialize(IFile a_File)
        {
            Version = a_File.Read<short>();
        }

        public override void Serialize(IFile a_File)
        {
            a_File.Write<short>(Version);
        }

        public override int Size
        {
            get
            {
                return 2;
            }
        }
    }
    public class gCInfoCommandKillNPC : classData
    {
        short Version;

        public override void deSerialize(IFile a_File)
        {
            Version = a_File.Read<short>();
        }

        public override void Serialize(IFile a_File)
        {
            a_File.Write<short>(Version);
        }

        public override int Size
        {
            get
            {
                return 2;
            }
        }
    }
    public class gCInfoCommandDestroyItem : classData
    {
        short Version;

        public override void deSerialize(IFile a_File)
        {
            Version = a_File.Read<short>();
        }

        public override void Serialize(IFile a_File)
        {
            a_File.Write<short>(Version);
        }

        public override int Size
        {
            get
            {
                return 2;
            }
        }
    }
    public class gCInfoCommandSetSectorStatus : classData
    {
        short Version;

        public override void deSerialize(IFile a_File)
        {
            Version = a_File.Read<short>();
        }

        public override void Serialize(IFile a_File)
        {
            a_File.Write<short>(Version);
        }

        public override int Size
        {
            get
            {
                return 2;
            }
        }
    }
    public class gCInfoCommandSetMobEnabled : classData
    {
        short Version;

        public override void deSerialize(IFile a_File)
        {
            Version = a_File.Read<short>();
        }

        public override void Serialize(IFile a_File)
        {
            a_File.Write<short>(Version);
        }

        public override int Size
        {
            get
            {
                return 2;
            }
        }
    }
    public class gCInfoCommandBack : classData
    {
        short Version;

        public override void deSerialize(IFile a_File)
        {
            Version = a_File.Read<short>();
        }

        public override void Serialize(IFile a_File)
        {
            a_File.Write<short>(Version);
        }

        public override int Size
        {
            get
            {
                return 2;
            }
        }
    }
    public class gCInfoCommandShowPicture : classData
    {
        short Version;

        public override void deSerialize(IFile a_File)
        {
            Version = a_File.Read<short>();
        }

        public override void Serialize(IFile a_File)
        {
            a_File.Write<short>(Version);
        }

        public override int Size
        {
            get
            {
                return 2;
            }
        }
    }
    public class gCInfoCommandStartEffect : classData
    {
        short Version;

        public override void deSerialize(IFile a_File)
        {
            Version = a_File.Read<short>();
        }

        public override void Serialize(IFile a_File)
        {
            a_File.Write<short>(Version);
        }

        public override int Size
        {
            get
            {
                return 2;
            }
        }
    }
    public class gCInfoCommandShowSubtitle : classData
    {
        short Version;

        public override void deSerialize(IFile a_File)
        {
            Version = a_File.Read<short>();
        }

        public override void Serialize(IFile a_File)
        {
            a_File.Write<short>(Version);
        }

        public override int Size
        {
            get
            {
                return 2;
            }
        }
    }
    public class gCInfoCommandSetSlaveryEnabled : classData
    {
        short Version;

        public override void deSerialize(IFile a_File)
        {
            Version = a_File.Read<short>();
        }

        public override void Serialize(IFile a_File)
        {
            a_File.Write<short>(Version);
        }

        public override int Size
        {
            get
            {
                return 2;
            }
        }
    }
    public class gCInfoCommandThink : classData
    {
        short Version;

        public override void deSerialize(IFile a_File)
        {
            Version = a_File.Read<short>();
        }

        public override void Serialize(IFile a_File)
        {
            a_File.Write<short>(Version);
        }

        public override int Size
        {
            get
            {
                return 2;
            }
        }
    }
    public class gCInfoCommandRemoveNPC : classData
    {
        short Version;

        public override void deSerialize(IFile a_File)
        {
            Version = a_File.Read<short>();
        }

        public override void Serialize(IFile a_File)
        {
            a_File.Write<short>(Version);
        }

        public override int Size
        {
            get
            {
                return 2;
            }
        }
    }
    public class gCInfoCommandStopFixCamera : classData
    {
        short Version;

        public override void deSerialize(IFile a_File)
        {
            Version = a_File.Read<short>();
        }

        public override void Serialize(IFile a_File)
        {
            a_File.Write<short>(Version);
        }

        public override int Size
        {
            get
            {
                return 2;
            }
        }
    }
#endregion
}
