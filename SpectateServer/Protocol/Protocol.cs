using System;
using System.Collections.Generic;

namespace Protocol
{
    public static class Signals
    {
        public static readonly byte[] Ping = { 1, 0, 0, 0, 0, 0, 0, 0 };
        public static readonly byte[] Pong = { 2, 0, 0, 0, 0, 0, 0, 0 };
        public static readonly byte[] RequestEverything = { 10, 0, 0, 0, 0, 0, 0, 0 };
        public static readonly byte[] BeginGame = { 6, 0, 0, 0, 0, 0, 0, 0 };
    }

        enum ChannelID
        {
            Unused,

            Ping,				//c2s: <signal> sent once every two seconds, connection terminated after 10 seconds of inactivity
            Pong,				//s2c: <signal> sent as a response to Ping

            ControlMessage,		//s2c: ControlMessage
            GameProgress,		//s2c: GameProgress

            PhaseChange,		//s2c: PhaseChange
            BeginGame,			//s2c: <signal> //indicates that the player faction is fully initialized and transferred, and the game should begin. Mostly important for AI, only

            Hello,				//c2s: Hello -> s2c: Result
            ClientFaction,		//c2s: ClientFactionRequest -> s2c: ClientFactionResponse
            SetFactionID,		//s2c: NET(TUniqueID)

            RequestEverything,	//c2s: <signal> -> [everything] //(only if spectator) 
            BeginEverything,	//s2c: <signal> //indicates that now everything will be (re)sent
            EndEverything,		//s2c: <signal> //indicates that the server is done sending everything, and normal delta updates will resume

            WorldDelta,			//s2c : WorldDelta

            Explosion,			//s2c : Explosion

            NewMobileEntity,	//s2c: NewMobileEntity
            MobileState,		//s2c: MobileState
            MobileEntityMotion,		//s2c: MobileEntityMotion
            PreciseAttackTrajectory,	//s2c: PreciseAttackTrajectory
            VagueAttackTrajectory,		//s2c: VagueAttackTrajectory
            EntityTurretRotation,	//s2c: EntityTurretRotation

            ChangeEntityOwner,	//s2c: ChangeEntityOwner

            RemoveEntity,		//c2s: TUniqueID
            RestoreCoreLink,	//c2s: TUniqueID

            InstantiateStructure,	//s2c: StructureConstruction
            UpdateStructure,		//s2c: StructureState

            DestroyEntity,		//s2c: DestroyEntity

            ChoiceIntersection,	//c2s: ChoiceIntersection
            RequestRoundEnd,	//c2s: <signal>
            RequestContinueRound,	//c2s: <signal>

            EntityDirectCommand,//c2s: EntityDirectCommand

            DropLastTask,		//c2s: TUniqueID
            DropAllTasks,		//c2s: TUniqueID

            SignalNewJob,		//c2s: StandardJob
            SignalTerrainMod,	//c2s: TerrainMod

            ConstructionBegin,	//c2s: ConstructionBegin
            ConstructionAbort,	//c2s: ConstructionAbort

            EnableStructure,	//c2s: EnableStructure

            Automate,			//c2s: EntityStateUpdate
            Sleep,				//c2s: EntityStateUpdate

            ResourceUpdate,		//s2c: ResourceUpdate

            CreateMarker,		//c2s/s2c: Marker (non-dialogue)
            MarkerCreationFailedLimitReached,	//s2c: ClientMarker
            DestroyMarker,		//c2s/s2c: TUniqueID
            TouchMarker,		//c2s: TouchedMarker

            IdentifyUser,		// [c2s: TUniqueID -> ] s2c: IdentifyUser
            ChangeMyName,		// c2s: ClientName
            FactionOnline,		// s2c: TUniqueID
            FactionOffline,		// s2c: TUniqueID
            FactionRequestedNewRound,	// s2c: TUniqueID
            FactionContinuedRound,		// s2c: TUniqueID

            LockEMPZones,		// s2c : UINT16
            UplinkBandwidth,	// s2c : int, can be negative to indicate it's disabled
            UnitLimits,			// s2c : UnitLimits

            ChatMessage,		// c2s: NetString<256>

            //remote control channels (these require user level 2+)
            RC_UpdateWorld,				//c2s: Grid::TExtendedPlanetConfig
            RC_Lockdown,				//c2s: bool -> s2c: bool
            RC_Kick,					//c2s: pair<TUniqueID,bool>	(kick / kick and ban)
            RC_KickAllNonPrivileged,	//c2s: <signal>
        };
        enum InfoChannelID
        {
            ServerInfo,	//s2c: ServerInfo
            Ping,
        };

        public struct ServerInfo
        {
            public THostBroadcastPacket hostInfo;
            public GameInfo gameInfo;

        }
        public struct Ping
        {
            public UInt64 ping;
        }

        public struct PhaseChange
        {
            public byte newPhase;
            public UInt32 roundNumber;
            public float timeFactor;
        }
        public struct THostBroadcastPacket
	    {
            public TUniqueID hostID;
            public UInt64 version1;
            public UInt64 version2;
            public UInt64 version3;
            public UInt64 version4;
            public UInt64 version5;
            public UInt64 version6;
            public UInt64 version7;
            public UInt64 version8;
            public UInt16 serverBasePort;
	    };

        public struct ClientFactionRequest
        {
            public UInt32 startFactionTypes;
            public CryptographicID factionToken;
        }

        public struct ClientFactionResponse
        {
            public UInt32 startFactionTypes;
            public CryptographicID factionToken;
            public PlanetConfig planetConfig;
            public GameInfo info;
        }

        public struct PlanetConfig
        {
            public UInt32 innerCoreType;
            public Vector color;
            public Vector sunPlane;
            public Vector sunColor;
        }

        public struct Vector
        {
            public float x;
            public float y;
            public float z;
            public float w;

        }
        public struct GameInfo
        {
            public UInt16 numFactionsOnline;
            public UInt16 numFactionsCreated;
            public UInt32 roundNumber;
            public TUniqueID worldID;
            public byte activePhase;
            public UInt32 gameFlags;
            public byte gameGoal;
        }

        public enum gameFlags
        {
            ClosedGame = 0x1,
            BasesRequired = 0x2,
            StartBases = 0x4,
            NexusAI = 0x8,
            EMPZones = 0x10,

            SupportSpectatorFactions = 0x100,
            SupportRegularFactions = 0x200,

        };

        public struct TUniqueID
        {
            public UInt64 first;
            public UInt64 second;

            public static TUniqueID generate()
            {
                byte[] id = Guid.NewGuid().ToByteArray();
                TUniqueID uid = new TUniqueID();
                uid.first= BitConverter.ToUInt64(id, 0);
                uid.second = BitConverter.ToUInt64(id, 8);
                return uid;
            }

        }

        public struct Result
        {
            public bool success;
            public byte[] message;
        }

        public struct CryptographicID
        {
            public UInt64 first;
            public UInt64 second;
            public UInt64 third;
            public UInt64 fourth;
        }

    
}
