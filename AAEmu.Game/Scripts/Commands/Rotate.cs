﻿using AAEmu.Game.Core.Managers;
using AAEmu.Game.Models.Game.Units.Movements;
using AAEmu.Game.Core.Packets.G2C;
using AAEmu.Game.Models.Game;
using AAEmu.Game.Models.Game.Char;
using AAEmu.Game.Utils;
using AAEmu.Commons.Utils;
using NLog;
using AAEmu.Game.Utils.Scripts;

namespace AAEmu.Game.Scripts.Commands;

public class Rotate : ICommand
{
    protected static Logger Logger = LogManager.GetCurrentClassLogger();
    public void OnLoad()
    {
        CommandManager.Instance.Register("rotate", this);
    }

    public string GetCommandLineHelp()
    {
        return "[angle]";
    }

    public string GetCommandHelpText()
    {
        return "Rotate target unit towards you, or set it's local rotation to a given angle";
    }

    public void Execute(Character character, string[] args, IMessageOutput messageOutput)
    {
        //if (args.Length < 2)
        //{
        //    character.SendMessage("[Rotate] /rotate <objType: npc, doodad> <objId>");
        //    return;
        //}

        if (character.CurrentTarget != null)
        {
            character.SendMessage($"[Rotate] Unit: {character.CurrentTarget.Name}, ObjId: {character.CurrentTarget.ObjId}");

            var Seq = (uint)Rand.Next(0, 10000);
            var moveType = (UnitMoveType)MoveType.GetType(MoveTypeEnum.Unit);

            moveType.X = character.CurrentTarget.Transform.World.Position.X;
            moveType.Y = character.CurrentTarget.Transform.World.Position.Y;
            moveType.Z = character.CurrentTarget.Transform.World.Position.Z;

            var angle = (float)MathUtil.CalculateAngleFrom(character.CurrentTarget.Transform.World.Position, character.Transform.World.Position) - 90f;
            var rotZ = MathUtil.ConvertDegreeToSByteDirection(angle);
            /*
            if (args.Length > 0)
            {
                if (!sbyte.TryParse(args[0], out rotZ))
                    character.SendMessage("Rotation sbyte out of range");
            }

            var angle2 = angle;
            angle = (float)MathUtil.ConvertSbyteDirectionToDegree(rotZ);
            */
            character.CurrentTarget.Transform.Local.SetRotationDegree(0f, 0f, angle);

            //character.CurrentTarget.Transform.Local.LookAt(character.Transform.Local.Position);

            moveType.RotationX = MathUtil.ConvertRadianToDirection(character.CurrentTarget.Transform.Local.Rotation.X);
            moveType.RotationY = MathUtil.ConvertRadianToDirection(character.CurrentTarget.Transform.Local.Rotation.Y);
            moveType.RotationZ = MathUtil.ConvertRadianToDirection(character.CurrentTarget.Transform.Local.Rotation.Z);
            //moveType.RotationZ = rotZ;

            moveType.ActorFlags = 5;
            moveType.DeltaMovement = new sbyte[3];
            moveType.DeltaMovement[0] = 0;
            moveType.DeltaMovement[1] = 0;
            moveType.DeltaMovement[2] = 0;
            moveType.Stance = 1; //combat=0, idle=1
            moveType.Alertness = 0; //idle=0, combat=2
            moveType.Time += 50; // has to change all the time for normal motion.

            character.BroadcastPacket(new SCOneUnitMovementPacket(character.CurrentTarget.ObjId, moveType), true);
            character.SendMessage($"New rotation {angle}° ({character.CurrentTarget.Transform.Local.Rotation.Z:0.00} rad, sbyte {rotZ}) for {character.CurrentTarget.ObjId}");
            character.SendMessage($"New position {character.CurrentTarget.Transform.Local}");
        }
        else
            character.SendMessage("[Rotate] You need to target something first");
    }
}
