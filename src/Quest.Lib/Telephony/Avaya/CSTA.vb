Imports System
Imports System.Runtime.InteropServices

Namespace CTI
    Public NotInheritable Class Csta
        Private Sub New()
        End Sub
        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure Nulltype
            ' null
            Event null()
        End Structure
        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure DeviceID_t
            <MarshalAs(UnmanagedType.ByValArray, SizeConst:=64)> _
            Public device As Char()
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure ServerID_t
            Public server As Char()
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure AppName_t
            Public appName As Char()
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure Version_t
            Public version As Char()
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure LoginID_t
            Public login As Char()
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure AgentID_t
            Public agent As Char()
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure AgentPassword_t
            Public password As Char()
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure AccountInfo_t
            Public account As Char()
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure AuthCode_t
            Public code As Char()
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure WinNTPipe_t
            Public pipe As Char()
        End Structure

        Public Const ACS_OPEN_STREAM As Integer = 1
        Public Const ACS_OPEN_STREAM_CONF As Integer = 2
        Public Const ACS_CLOSE_STREAM As Integer = 3
        Public Const ACS_CLOSE_STREAM_CONF As Integer = 4
        Public Const ACS_ABORT_STREAM As Integer = 5
        Public Const ACS_UNIVERSAL_FAILURE_CONF As Integer = 6
        Public Const ACS_UNIVERSAL_FAILURE As Integer = 7
        Public Const ACS_KEY_REQUEST As Integer = 8
        Public Const ACS_KEY_REPLY As Integer = 9
        Public Const ACS_NAME_SRV_REQUEST As Integer = 10
        Public Const ACS_NAME_SRV_REPLY As Integer = 11
        Public Const ACS_AUTH_REPLY As Integer = 12
        Public Const ACS_AUTH_REPLY_TWO As Integer = 13
        Public Enum StreamType_t
            ST_CSTA = 1
            ST_OAM = 2
            ST_DIRECTORY = 3
            ST_NMSRV = 4
        End Enum

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CryptPasswd_t
            Public length As Short
            <MarshalAs(UnmanagedType.ByValArray, SizeConst:=47)> _
            Public value As Char()
        End Structure

        Public Enum Level_t
            ACS_LEVEL1 = 1
            ACS_LEVEL2 = 2
            ACS_LEVEL3 = 3
            ACS_LEVEL4 = 4
        End Enum

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure ACSOpenStream_t
            Public streamType As StreamType_t
            Public serverID As ServerID_t
            Public loginID As LoginID_t
            Public cryptPass As CryptPasswd_t
            Public applicationName As AppName_t
            Public level As Level_t
            Public apiVer As Version_t
            Public libVer As Version_t
            Public tsrvVer As Version_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure ACSOpenStreamConfEvent_t
            Public apiVer As Version_t
            Public libVer As Version_t
            Public tsrvVer As Version_t
            Public drvrVer As Version_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure ACSCloseStream_t
            Public nil As Nulltype
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure ACSCloseStreamConfEvent_t
            Public nil As Nulltype
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure ACSAbortStream_t
            Public nil As Nulltype
        End Structure

        Public Enum ACSUniversalFailure_t
            TSERVER_STREAM_FAILED = 0
            TSERVER_NO_THREAD = 1
            TSERVER_BAD_DRIVER_ID = 2
            TSERVER_DEAD_DRIVER = 3
            TSERVER_MESSAGE_HIGH_WATER_MARK = 4
            TSERVER_FREE_BUFFER_FAILED = 5
            TSERVER_SEND_TO_DRIVER = 6
            TSERVER_RECEIVE_FROM_DRIVER = 7
            TSERVER_REGISTRATION_FAILED = 8
            TSERVER_SPX_FAILED = 9
            TSERVER_TRACE = 10
            TSERVER_NO_MEMORY = 11
            TSERVER_ENCODE_FAILED = 12
            TSERVER_DECODE_FAILED = 13
            TSERVER_BAD_CONNECTION = 14
            TSERVER_BAD_PDU = 15
            TSERVER_NO_VERSION = 16
            TSERVER_ECB_MAX_EXCEEDED = 17
            TSERVER_NO_ECBS = 18
            TSERVER_NO_SDB = 19
            TSERVER_NO_SDB_CHECK_NEEDED = 20
            TSERVER_SDB_CHECK_NEEDED = 21
            TSERVER_BAD_SDB_LEVEL = 22
            TSERVER_BAD_SERVERID = 23
            TSERVER_BAD_STREAM_TYPE = 24
            TSERVER_BAD_PASSWORD_OR_LOGIN = 25
            TSERVER_NO_USER_RECORD = 26
            TSERVER_NO_DEVICE_RECORD = 27
            TSERVER_DEVICE_NOT_ON_LIST = 28
            TSERVER_USERS_RESTRICTED_HOME = 30
            TSERVER_NO_AWAYPERMISSION = 31
            TSERVER_NO_HOMEPERMISSION = 32
            TSERVER_NO_AWAY_WORKTOP = 33
            TSERVER_BAD_DEVICE_RECORD = 34
            TSERVER_DEVICE_NOT_SUPPORTED = 35
            TSERVER_INSUFFICIENT_PERMISSION = 36
            TSERVER_NO_RESOURCE_TAG = 37
            TSERVER_INVALID_MESSAGE = 38
            TSERVER_EXCEPTION_LIST = 39
            TSERVER_NOT_ON_OAM_LIST = 40
            TSERVER_PBX_ID_NOT_IN_SDB = 41
            TSERVER_USER_LICENSES_EXCEEDED = 42
            TSERVER_OAM_DROP_CONNECTION = 43
            TSERVER_NO_VERSION_RECORD = 44
            TSERVER_OLD_VERSION_RECORD = 45
            TSERVER_BAD_PACKET = 46
            TSERVER_OPEN_FAILED = 47
            TSERVER_OAM_IN_USE = 48
            TSERVER_DEVICE_NOT_ON_HOME_LIST = 49
            TSERVER_DEVICE_NOT_ON_CALL_CONTROL_LIST = 50
            TSERVER_DEVICE_NOT_ON_AWAY_LIST = 51
            TSERVER_DEVICE_NOT_ON_ROUTE_LIST = 52
            TSERVER_DEVICE_NOT_ON_MONITOR_DEVICE_LIST = 53
            TSERVER_DEVICE_NOT_ON_MONITOR_CALL_DEVICE_LIST = 54
            TSERVER_NO_CALL_CALL_MONITOR_PERMISSION = 55
            TSERVER_HOME_DEVICE_LIST_EMPTY = 56
            TSERVER_CALL_CONTROL_LIST_EMPTY = 57
            TSERVER_AWAY_LIST_EMPTY = 58
            TSERVER_ROUTE_LIST_EMPTY = 59
            TSERVER_MONITOR_DEVICE_LIST_EMPTY = 60
            TSERVER_MONITOR_CALL_DEVICE_LIST_EMPTY = 61
            TSERVER_USER_AT_HOME_WORKTOP = 62
            TSERVER_DEVICE_LIST_EMPTY = 63
            TSERVER_BAD_GET_DEVICE_LEVEL = 64
            TSERVER_DRIVER_UNREGISTERED = 65
            TSERVER_NO_ACS_STREAM = 66
            TSERVER_DROP_OAM = 67
            TSERVER_ECB_TIMEOUT = 68
            TSERVER_BAD_ECB = 69
            TSERVER_ADVERTISE_FAILED = 70
            TSERVER_NETWARE_FAILURE = 71
            TSERVER_TDI_QUEUE_FAULT = 72
            TSERVER_DRIVER_CONGESTION = 73
            TSERVER_NO_TDI_BUFFERS = 74
            TSERVER_OLD_INVOKEID = 75
            TSERVER_HWMARK_TO_LARGE = 76
            TSERVER_SET_ECB_TO_LOW = 77
            TSERVER_NO_RECORD_IN_FILE = 78
            TSERVER_ECB_OVERDUE = 79
            TSERVER_BAD_PW_ENCRYPTION = 80
            TSERVER_BAD_TSERV_PROTOCOL = 81
            TSERVER_BAD_DRIVER_PROTOCOL = 82
            TSERVER_BAD_TRANSPORT_TYPE = 83
            TSERVER_PDU_VERSION_MISMATCH = 84
            TSERVER_VERSION_MISMATCH = 85
            TSERVER_LICENSE_MISMATCH = 86
            TSERVER_BAD_ATTRIBUTE_LIST = 87
            TSERVER_BAD_TLIST_TYPE = 88
            TSERVER_BAD_PROTOCOL_FORMAT = 89
            TSERVER_OLD_TSLIB = 90
            TSERVER_BAD_LICENSE_FILE = 91
            TSERVER_NO_PATCHES = 92
            TSERVER_SYSTEM_ERROR = 93
            TSERVER_OAM_LIST_EMPTY = 94
            TSERVER_TCP_FAILED = 95
            TSERVER_SPX_DISABLED = 96
            TSERVER_TCP_DISABLED = 97
            TSERVER_REQUIRED_MODULES_NOT_LOADED = 98
            TSERVER_TRANSPORT_IN_USE_BY_OAM = 99
            TSERVER_NO_NDS_OAM_PERMISSION = 100
            TSERVER_OPEN_SDB_LOG_FAILED = 101
            TSERVER_INVALID_LOG_SIZE = 102
            TSERVER_WRITE_SDB_LOG_FAILED = 103
            TSERVER_NT_FAILURE = 104
            TSERVER_LOAD_LIB_FAILED = 105
            TSERVER_INVALID_DRIVER = 106
            TSERVER_REGISTRY_ERROR = 107
            TSERVER_DUPLICATE_ENTRY = 108
            TSERVER_DRIVER_LOADED = 109
            TSERVER_DRIVER_NOT_LOADED = 110
            TSERVER_NO_LOGON_PERMISSION = 111
            TSERVER_ACCOUNT_DISABLED = 112
            TSERVER_NO_NETLOGON = 113
            TSERVER_ACCT_RESTRICTED = 114
            TSERVER_INVALID_LOGON_TIME = 115
            TSERVER_INVALID_WORKSTATION = 116
            TSERVER_ACCT_LOCKED_OUT = 117
            TSERVER_PASSWORD_EXPIRED = 118
            DRIVER_DUPLICATE_ACSHANDLE = 1000
            DRIVER_INVALID_ACS_REQUEST = 1001
            DRIVER_ACS_HANDLE_REJECTION = 1002
            DRIVER_INVALID_CLASS_REJECTION = 1003
            DRIVER_GENERIC_REJECTION = 1004
            DRIVER_RESOURCE_LIMITATION = 1005
            DRIVER_ACSHANDLE_TERMINATION = 1006
            DRIVER_LINK_UNAVAILABLE = 1007
            DRIVER_OAM_IN_USE = 1008
        End Enum

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure ACSUniversalFailureConfEvent_t
            Public [error] As ACSUniversalFailure_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure ACSUniversalFailureEvent_t
            Public [error] As ACSUniversalFailure_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure ChallengeKey_t
            Public length As Short
            <MarshalAs(UnmanagedType.ByValArray, SizeConst:=8)> _
            Public value As Char()
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure ACSKeyRequest_t
            Public loginID As LoginID_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure ACSKeyReply_t
            Public objectID As Integer
            Public key As ChallengeKey_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure ACSNameSrvRequest_t
            Public streamType As StreamType_t
        End Structure

        ' *****************************
        ' * Nested elements not supported
        ' *****************************
        ' typedef struct ACSNameAddr_t {
        '     char            FAR *serverName;
        '     struct {
        '         short           length;
        '         unsigned char   FAR *value;
        '     } serverAddr;
        ' } ACSNameAddr_t;
        ' *****************************

        ' *****************************
        ' * Nested elements not supported
        ' *****************************
        ' typedef struct ACSNameSrvReply_t {
        '     Boolean         more;
        '     struct {
        '         short           count;
        '         ACSNameAddr_t   FAR *nameAddr;
        '     } list;
        ' } ACSNameSrvReply_t;
        ' *****************************

        Public Enum ACSAuthType_t
            REQUIRES_EXTERNAL_AUTH = -1
            AUTH_LOGIN_ID_ONLY = 0
            AUTH_LOGIN_ID_IS_DEFAULT = 1
            NEED_LOGIN_ID_AND_PASSWD = 2
            ANY_LOGIN_ID = 3
        End Enum

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure ACSAuthInfo_t
            Public authType As ACSAuthType_t
            Public authLoginID As LoginID_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure ACSAuthReply_t
            Public objectID As Integer
            Public key As ChallengeKey_t
            Public authInfo As ACSAuthInfo_t
        End Structure

        Public Enum ACSEncodeType_t
            CAN_USE_BINDERY_ENCRYPTION = 1
            NDS_AUTH_CONNID = 2
            WIN_NT_LOCAL = 3
            WIN_NT_NAMED_PIPE = 4
            WIN_NT_WRITE_DATA = 5
        End Enum

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure ACSAuthReplyTwo_t
            Public objectID As Integer
            Public key As ChallengeKey_t
            Public authInfo As ACSAuthInfo_t
            Public encodeType As ACSEncodeType_t
            Public pipe As WinNTPipe_t
        End Structure

        ' Generated by PInvoke Wizard (v 1.3) from The Paul Yao Company http://www.paulyao.com 

        Public Const CSTA_ALTERNATE_CALL As Integer = 1
        Public Const CSTA_ALTERNATE_CALL_CONF As Integer = 2
        Public Const CSTA_ANSWER_CALL As Integer = 3
        Public Const CSTA_ANSWER_CALL_CONF As Integer = 4
        Public Const CSTA_CALL_COMPLETION As Integer = 5
        Public Const CSTA_CALL_COMPLETION_CONF As Integer = 6
        Public Const CSTA_CLEAR_CALL As Integer = 7
        Public Const CSTA_CLEAR_CALL_CONF As Integer = 8
        Public Const CSTA_CLEAR_CONNECTION As Integer = 9
        Public Const CSTA_CLEAR_CONNECTION_CONF As Integer = 10
        Public Const CSTA_CONFERENCE_CALL As Integer = 11
        Public Const CSTA_CONFERENCE_CALL_CONF As Integer = 12
        Public Const CSTA_CONSULTATION_CALL As Integer = 13
        Public Const CSTA_CONSULTATION_CALL_CONF As Integer = 14
        Public Const CSTA_DEFLECT_CALL As Integer = 15
        Public Const CSTA_DEFLECT_CALL_CONF As Integer = 16
        Public Const CSTA_PICKUP_CALL As Integer = 17
        Public Const CSTA_PICKUP_CALL_CONF As Integer = 18
        Public Const CSTA_GROUP_PICKUP_CALL As Integer = 19
        Public Const CSTA_GROUP_PICKUP_CALL_CONF As Integer = 20
        Public Const CSTA_HOLD_CALL As Integer = 21
        Public Const CSTA_HOLD_CALL_CONF As Integer = 22
        Public Const CSTA_MAKE_CALL As Integer = 23
        Public Const CSTA_MAKE_CALL_CONF As Integer = 24
        Public Const CSTA_MAKE_PREDICTIVE_CALL As Integer = 25
        Public Const CSTA_MAKE_PREDICTIVE_CALL_CONF As Integer = 26
        Public Const CSTA_QUERY_MWI As Integer = 27
        Public Const CSTA_QUERY_MWI_CONF As Integer = 28
        Public Const CSTA_QUERY_DND As Integer = 29
        Public Const CSTA_QUERY_DND_CONF As Integer = 30
        Public Const CSTA_QUERY_FWD As Integer = 31
        Public Const CSTA_QUERY_FWD_CONF As Integer = 32
        Public Const CSTA_QUERY_AGENT_STATE As Integer = 33
        Public Const CSTA_QUERY_AGENT_STATE_CONF As Integer = 34
        Public Const CSTA_QUERY_LAST_NUMBER As Integer = 35
        Public Const CSTA_QUERY_LAST_NUMBER_CONF As Integer = 36
        Public Const CSTA_QUERY_DEVICE_INFO As Integer = 37
        Public Const CSTA_QUERY_DEVICE_INFO_CONF As Integer = 38
        Public Const CSTA_RECONNECT_CALL As Integer = 39
        Public Const CSTA_RECONNECT_CALL_CONF As Integer = 40
        Public Const CSTA_RETRIEVE_CALL As Integer = 41
        Public Const CSTA_RETRIEVE_CALL_CONF As Integer = 42
        Public Const CSTA_SET_MWI As Integer = 43
        Public Const CSTA_SET_MWI_CONF As Integer = 44
        Public Const CSTA_SET_DND As Integer = 45
        Public Const CSTA_SET_DND_CONF As Integer = 46
        Public Const CSTA_SET_FWD As Integer = 47
        Public Const CSTA_SET_FWD_CONF As Integer = 48
        Public Const CSTA_SET_AGENT_STATE As Integer = 49
        Public Const CSTA_SET_AGENT_STATE_CONF As Integer = 50
        Public Const CSTA_TRANSFER_CALL As Integer = 51
        Public Const CSTA_TRANSFER_CALL_CONF As Integer = 52
        Public Const CSTA_UNIVERSAL_FAILURE_CONF As Integer = 53
        Public Const CSTA_CALL_CLEARED As Integer = 54
        Public Const CSTA_CONFERENCED As Integer = 55
        Public Const CSTA_CONNECTION_CLEARED As Integer = 56
        Public Const CSTA_DELIVERED As Integer = 57
        Public Const CSTA_DIVERTED As Integer = 58
        Public Const CSTA_ESTABLISHED As Integer = 59
        Public Const CSTA_FAILED As Integer = 60
        Public Const CSTA_HELD As Integer = 61
        Public Const CSTA_NETWORK_REACHED As Integer = 62
        Public Const CSTA_ORIGINATED As Integer = 63
        Public Const CSTA_QUEUED As Integer = 64
        Public Const CSTA_RETRIEVED As Integer = 65
        Public Const CSTA_SERVICE_INITIATED As Integer = 66
        Public Const CSTA_TRANSFERRED As Integer = 67
        Public Const CSTA_CALL_INFORMATION As Integer = 68
        Public Const CSTA_DO_NOT_DISTURB As Integer = 69
        Public Const CSTA_FORWARDING As Integer = 70
        Public Const CSTA_MESSAGE_WAITING As Integer = 71
        Public Const CSTA_LOGGED_ON As Integer = 72
        Public Const CSTA_LOGGED_OFF As Integer = 73
        Public Const CSTA_NOT_READY As Integer = 74
        Public Const CSTA_READY As Integer = 75
        Public Const CSTA_WORK_NOT_READY As Integer = 76
        Public Const CSTA_WORK_READY As Integer = 77
        Public Const CSTA_ROUTE_REGISTER_REQ As Integer = 78
        Public Const CSTA_ROUTE_REGISTER_REQ_CONF As Integer = 79
        Public Const CSTA_ROUTE_REGISTER_CANCEL As Integer = 80
        Public Const CSTA_ROUTE_REGISTER_CANCEL_CONF As Integer = 81
        Public Const CSTA_ROUTE_REGISTER_ABORT As Integer = 82
        Public Const CSTA_ROUTE_REQUEST As Integer = 83
        Public Const CSTA_ROUTE_SELECT_REQUEST As Integer = 84
        Public Const CSTA_RE_ROUTE_REQUEST As Integer = 85
        Public Const CSTA_ROUTE_USED As Integer = 86
        Public Const CSTA_ROUTE_END As Integer = 87
        Public Const CSTA_ROUTE_END_REQUEST As Integer = 88
        Public Const CSTA_ESCAPE_SVC As Integer = 89
        Public Const CSTA_ESCAPE_SVC_CONF As Integer = 90
        Public Const CSTA_ESCAPE_SVC_REQ As Integer = 91
        Public Const CSTA_ESCAPE_SVC_REQ_CONF As Integer = 92
        Public Const CSTA_PRIVATE As Integer = 93
        Public Const CSTA_PRIVATE_STATUS As Integer = 94
        Public Const CSTA_SEND_PRIVATE As Integer = 95
        Public Const CSTA_BACK_IN_SERVICE As Integer = 96
        Public Const CSTA_OUT_OF_SERVICE As Integer = 97
        Public Const CSTA_REQ_SYS_STAT As Integer = 98
        Public Const CSTA_SYS_STAT_REQ_CONF As Integer = 99
        Public Const CSTA_SYS_STAT_START As Integer = 100
        Public Const CSTA_SYS_STAT_START_CONF As Integer = 101
        Public Const CSTA_SYS_STAT_STOP As Integer = 102
        Public Const CSTA_SYS_STAT_STOP_CONF As Integer = 103
        Public Const CSTA_CHANGE_SYS_STAT_FILTER As Integer = 104
        Public Const CSTA_CHANGE_SYS_STAT_FILTER_CONF As Integer = 105
        Public Const CSTA_SYS_STAT As Integer = 106
        Public Const CSTA_SYS_STAT_ENDED As Integer = 107
        Public Const CSTA_SYS_STAT_REQ As Integer = 108
        Public Const CSTA_REQ_SYS_STAT_CONF As Integer = 109
        Public Const CSTA_SYS_STAT_EVENT_SEND As Integer = 110
        Public Const CSTA_MONITOR_DEVICE As Integer = 111
        Public Const CSTA_MONITOR_CALL As Integer = 112
        Public Const CSTA_MONITOR_CALLS_VIA_DEVICE As Integer = 113
        Public Const CSTA_MONITOR_CONF As Integer = 114
        Public Const CSTA_CHANGE_MONITOR_FILTER As Integer = 115
        Public Const CSTA_CHANGE_MONITOR_FILTER_CONF As Integer = 116
        Public Const CSTA_MONITOR_STOP As Integer = 117
        Public Const CSTA_MONITOR_STOP_CONF As Integer = 118
        Public Const CSTA_MONITOR_ENDED As Integer = 119
        Public Const CSTA_SNAPSHOT_CALL As Integer = 120
        Public Const CSTA_SNAPSHOT_CALL_CONF As Integer = 121
        Public Const CSTA_SNAPSHOT_DEVICE As Integer = 122
        Public Const CSTA_SNAPSHOT_DEVICE_CONF As Integer = 123
        Public Const CSTA_GETAPI_CAPS As Integer = 124
        Public Const CSTA_GETAPI_CAPS_CONF As Integer = 125
        Public Const CSTA_GET_DEVICE_LIST As Integer = 126
        Public Const CSTA_GET_DEVICE_LIST_CONF As Integer = 127
        Public Const CSTA_QUERY_CALL_MONITOR As Integer = 128
        Public Const CSTA_QUERY_CALL_MONITOR_CONF As Integer = 129
        Public Const CSTA_ROUTE_REQUEST_EXT As Integer = 130
        Public Const CSTA_ROUTE_USED_EXT As Integer = 131
        Public Const CSTA_ROUTE_SELECT_INV_REQUEST As Integer = 132
        Public Const CSTA_ROUTE_END_INV_REQUEST As Integer = 133
        Public Enum CSTAUniversalFailure_t
            GENERIC_UNSPECIFIED = 0
            GENERIC_OPERATION = 1
            REQUEST_INCOMPATIBLE_WITH_OBJECT = 2
            VALUE_OUT_OF_RANGE = 3
            OBJECT_NOT_KNOWN = 4
            INVALID_CALLING_DEVICE = 5
            INVALID_CALLED_DEVICE = 6
            INVALID_FORWARDING_DESTINATION = 7
            PRIVILEGE_VIOLATION_ON_SPECIFIED_DEVICE = 8
            PRIVILEGE_VIOLATION_ON_CALLED_DEVICE = 9
            PRIVILEGE_VIOLATION_ON_CALLING_DEVICE = 10
            INVALID_CSTA_CALL_IDENTIFIER = 11
            INVALID_CSTA_DEVICE_IDENTIFIER = 12
            INVALID_CSTA_CONNECTION_IDENTIFIER = 13
            INVALID_DESTINATION = 14
            INVALID_FEATURE = 15
            INVALID_ALLOCATION_STATE = 16
            INVALID_CROSS_REF_ID = 17
            INVALID_OBJECT_TYPE = 18
            SECURITY_VIOLATION = 19
            GENERIC_STATE_INCOMPATIBILITY = 21
            INVALID_OBJECT_STATE = 22
            INVALID_CONNECTION_ID_FOR_ACTIVE_CALL = 23
            NO_ACTIVE_CALL = 24
            NO_HELD_CALL = 25
            NO_CALL_TO_CLEAR = 26
            NO_CONNECTION_TO_CLEAR = 27
            NO_CALL_TO_ANSWER = 28
            NO_CALL_TO_COMPLETE = 29
            GENERIC_SYSTEM_RESOURCE_AVAILABILITY = 31
            SERVICE_BUSY = 32
            RESOURCE_BUSY = 33
            RESOURCE_OUT_OF_SERVICE = 34
            NETWORK_BUSY = 35
            NETWORK_OUT_OF_SERVICE = 36
            OVERALL_MONITOR_LIMIT_EXCEEDED = 37
            CONFERENCE_MEMBER_LIMIT_EXCEEDED = 38
            GENERIC_SUBSCRIBED_RESOURCE_AVAILABILITY = 41
            OBJECT_MONITOR_LIMIT_EXCEEDED = 42
            EXTERNAL_TRUNK_LIMIT_EXCEEDED = 43
            OUTSTANDING_REQUEST_LIMIT_EXCEEDED = 44
            GENERIC_PERFORMANCE_MANAGEMENT = 51
            PERFORMANCE_LIMIT_EXCEEDED = 52
            UNSPECIFIED_SECURITY_ERROR = 60
            SEQUENCE_NUMBER_VIOLATED = 61
            TIME_STAMP_VIOLATED = 62
            PAC_VIOLATED = 63
            SEAL_VIOLATED = 64
            GENERIC_UNSPECIFIED_REJECTION = 70
            GENERIC_OPERATION_REJECTION = 71
            DUPLICATE_INVOCATION_REJECTION = 72
            UNRECOGNIZED_OPERATION_REJECTION = 73
            MISTYPED_ARGUMENT_REJECTION = 74
            RESOURCE_LIMITATION_REJECTION = 75
            ACS_HANDLE_TERMINATION_REJECTION = 76
            SERVICE_TERMINATION_REJECTION = 77
            REQUEST_TIMEOUT_REJECTION = 78
            REQUESTS_ON_DEVICE_EXCEEDED_REJECTION = 79
            UNRECOGNIZED_APDU_REJECTION = 80
            MISTYPED_APDU_REJECTION = 81
            BADLY_STRUCTURED_APDU_REJECTION = 82
            INITIATOR_RELEASING_REJECTION = 83
            UNRECOGNIZED_LINKEDID_REJECTION = 84
            LINKED_RESPONSE_UNEXPECTED_REJECTION = 85
            UNEXPECTED_CHILD_OPERATION_REJECTION = 86
            MISTYPED_RESULT_REJECTION = 87
            UNRECOGNIZED_ERROR_REJECTION = 88
            UNEXPECTED_ERROR_REJECTION = 89
            MISTYPED_PARAMETER_REJECTION = 90
            NON_STANDARD = 100
        End Enum

        Public Enum CSTAEventCause_t
            EC_NONE = -1
            EC_ACTIVE_MONITOR = 1
            EC_ALTERNATE = 2
            EC_BUSY = 3
            EC_CALL_BACK = 4
            EC_CALL_CANCELLED = 5
            EC_CALL_FORWARD_ALWAYS = 6
            EC_CALL_FORWARD_BUSY = 7
            EC_CALL_FORWARD_NO_ANSWER = 8
            EC_CALL_FORWARD = 9
            EC_CALL_NOT_ANSWERED = 10
            EC_CALL_PICKUP = 11
            EC_CAMP_ON = 12
            EC_DEST_NOT_OBTAINABLE = 13
            EC_DO_NOT_DISTURB = 14
            EC_INCOMPATIBLE_DESTINATION = 15
            EC_INVALID_ACCOUNT_CODE = 16
            EC_KEY_CONFERENCE = 17
            EC_LOCKOUT = 18
            EC_MAINTENANCE = 19
            EC_NETWORK_CONGESTION = 20
            EC_NETWORK_NOT_OBTAINABLE = 21
            EC_NEW_CALL = 22
            EC_NO_AVAILABLE_AGENTS = 23
            EC_OVERRIDE = 24
            EC_PARK = 25
            EC_OVERFLOW = 26
            EC_RECALL = 27
            EC_REDIRECTED = 28
            EC_REORDER_TONE = 29
            EC_RESOURCES_NOT_AVAILABLE = 30
            EC_SILENT_MONITOR = 31
            EC_TRANSFER = 32
            EC_TRUNKS_BUSY = 33
            EC_VOICE_UNIT_INITIATOR = 34
        End Enum

        Public Enum DeviceIDType_t
            DEVICE_IDENTIFIER = 0
            IMPLICIT_PUBLIC = 20
            EXPLICIT_PUBLIC_UNKNOWN = 30
            EXPLICIT_PUBLICintERNATIONAL = 31
            EXPLICIT_PUBLIC_NATIONAL = 32
            EXPLICIT_PUBLIC_NETWORK_SPECIFIC = 33
            EXPLICIT_PUBLIC_SUBSCRIBER = 34
            EXPLICIT_PUBLIC_ABBREVIATED = 35
            IMPLICIT_PRIVATE = 40
            EXPLICIT_PRIVATE_UNKNOWN = 50
            EXPLICIT_PRIVATE_LEVEL3_REGIONAL_NUMBER = 51
            EXPLICIT_PRIVATE_LEVEL2_REGIONAL_NUMBER = 52
            EXPLICIT_PRIVATE_LEVEL1_REGIONAL_NUMBER = 53
            EXPLICIT_PRIVATE_PTN_SPECIFIC_NUMBER = 54
            EXPLICIT_PRIVATE_LOCAL_NUMBER = 55
            EXPLICIT_PRIVATE_ABBREVIATED = 56
            OTHER_PLAN = 60
            TRUNK_IDENTIFIER = 70
            TRUNK_GROUP_IDENTIFIER = 71
        End Enum

        Public Enum DeviceIDStatus_t
            ID_PROVIDED = 0
            ID_NOT_KNOWN = 1
            ID_NOT_REQUIRED = 2
        End Enum

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure ExtendedDeviceID_t
            Public deviceID As DeviceID_t
            Public deviceIDType As DeviceIDType_t
            Public deviceIDStatus As DeviceIDStatus_t
        End Structure

        Public Enum ConnectionID_Device_t
            STATIC_ID = 0
            DYNAMIC_ID = 1
        End Enum

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure ConnectionID_t
            Public callID As UInt32
            Public deviceID As DeviceID_t
            Public devIDType As ConnectionID_Device_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure Connection_t
            Public party As ConnectionID_t
            Public staticDevice As ExtendedDeviceID_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure ConnectionList_t
            Public count As Integer
            Public connection As Connection_t
        End Structure

        Public Enum LocalConnectionState_t
            CS_NONE = -1
            CS_NULL = 0
            CS_INITIATE = 1
            CS_ALERTING = 2
            CS_CONNECT = 3
            CS_HOLD = 4
            CS_QUEUED = 5
            CS_FAIL = 6
        End Enum

        Public Const CF_CALL_CLEARED As Integer = 32768
        Public Const CF_CONFERENCED As Integer = 16384
        Public Const CF_CONNECTION_CLEARED As Integer = 8192
        Public Const CF_DELIVERED As Integer = 4096
        Public Const CF_DIVERTED As Integer = 2048
        Public Const CF_ESTABLISHED As Integer = 1024
        Public Const CF_FAILED As Integer = 512
        Public Const CF_HELD As Integer = 256
        Public Const CF_NETWORK_REACHED As Integer = 128
        Public Const CF_ORIGINATED As Integer = 64
        Public Const CF_QUEUED As Integer = 32
        Public Const CF_RETRIEVED As Integer = 16
        Public Const CF_SERVICE_INITIATED As Integer = 8
        Public Const CF_TRANSFERRED As Integer = 4
        Public Const FF_CALL_INFORMATION As Integer = 128
        Public Const FF_DO_NOT_DISTURB As Integer = 64
        Public Const FF_FORWARDING As Integer = 32
        Public Const FF_MESSAGE_WAITING As Integer = 16
        Public Const AF_LOGGED_ON As Integer = 128
        Public Const AF_LOGGED_OFF As Integer = 64
        Public Const AF_NOT_READY As Integer = 32
        Public Const AF_READY As Integer = 16
        Public Const AF_WORK_NOT_READY As Integer = 8
        Public Const AF_WORK_READY As Integer = 4
        Public Const MF_BACK_IN_SERVICE As Integer = 128
        Public Const MF_OUT_OF_SERVICE As Integer = 64
        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTAMonitorFilter_t
            Public [call] As UShort
            Public feature As Byte
            Public agent As Byte
            Public maintenance As Byte
            Public privateFilter As UInt32
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTACallState_t
            Public count As Integer
            Public state As LocalConnectionState_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTASnapshotDeviceResponseInfo_t
            Public callIdentifier As ConnectionID_t
            Public localCallState As CSTACallState_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTASnapshotDeviceData_t
            Public count As Integer
            Public info As CSTASnapshotDeviceResponseInfo_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTASnapshotCallResponseInfo_t
            Public deviceOnCall As ExtendedDeviceID_t
            Public callIdentifier As ConnectionID_t
            Public localConnectionState As LocalConnectionState_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTASnapshotCallData_t
            Public count As Integer
            Public info As CSTASnapshotCallResponseInfo_t
        End Structure

        Public Enum ForwardingType_t
            FWD_IMMEDIATE = 0
            FWD_BUSY = 1
            FWD_NO_ANS = 2
            FWD_BUSYint = 3
            FWD_BUSY_EXT = 4
            FWD_NO_ANSint = 5
            FWD_NO_ANS_EXT = 6
        End Enum

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure ForwardingInfo_t
            Public forwardingType As ForwardingType_t
            Public forwardingOn As Boolean
            Public forwardDN As DeviceID_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure ListForwardParameters_t
            Public count As Short
            <MarshalAs(UnmanagedType.ByValArray, SizeConst:=7)> _
            Public param As ForwardingInfo_t()
        End Structure

        Public Enum SelectValue_t
            SV_NORMAL = 0
            SV_LEAST_COST = 1
            SV_EMERGENCY = 2
            SV_ACD = 3
            SV_USER_DEFINED = 4
        End Enum

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure SetUpValues_t
            Public length As Integer
            Public value As Byte
        End Structure

        Public Const noListAvailable As Integer = -1
        Public Const noCountAvailable As Integer = -2

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTAAlternateCall_t
            Public activeCall As ConnectionID_t
            Public otherCall As ConnectionID_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTAAlternateCallConfEvent_t
            Public nil As Nulltype
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTAAnswerCall_t
            Public alertingCall As ConnectionID_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTAAnswerCallConfEvent_t
            Public nil As Nulltype
        End Structure

        Public Enum Feature_t
            FT_CAMP_ON = 0
            FT_CALL_BACK = 1
            FTintRUDE = 2
        End Enum

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTACallCompletion_t
            Public feature As Feature_t
            Public [call] As ConnectionID_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTACallCompletionConfEvent_t
            Public nil As Nulltype
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTAClearCall_t
            Public [call] As ConnectionID_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTAClearCallConfEvent_t
            Public nil As Nulltype
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTAClearConnection_t
            Public [call] As ConnectionID_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTAClearConnectionConfEvent_t
            Public nil As Nulltype
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTAConferenceCall_t
            Public heldCall As ConnectionID_t
            Public activeCall As ConnectionID_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTAConferenceCallConfEvent_t
            Public newCall As ConnectionID_t
            Public connList As ConnectionList_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTAConsultationCall_t
            Public activeCall As ConnectionID_t
            Public calledDevice As DeviceID_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTAConsultationCallConfEvent_t
            Public newCall As ConnectionID_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTADeflectCall_t
            Public deflectCall As ConnectionID_t
            Public calledDevice As DeviceID_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTADeflectCallConfEvent_t
            Public nil As Nulltype
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTAPickupCall_t
            Public deflectCall As ConnectionID_t
            Public calledDevice As DeviceID_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTAPickupCallConfEvent_t
            Public nil As Nulltype
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTAGroupPickupCall_t
            Public deflectCall As ConnectionID_t
            Public pickupDevice As DeviceID_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTAGroupPickupCallConfEvent_t
            Public nil As Nulltype
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTAHoldCall_t
            Public activeCall As ConnectionID_t
            Public reservation As Boolean
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTAHoldCallConfEvent_t
            Public nil As Nulltype
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTAMakeCall_t
            Public callingDevice As DeviceID_t
            Public calledDevice As DeviceID_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTAMakeCallConfEvent_t
            Public newCall As ConnectionID_t
        End Structure

        Public Enum AllocationState_t
            AS_CALL_DELIVERED = 0
            AS_CALL_ESTABLISHED = 1
        End Enum

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTAMakePredictiveCall_t
            Public callingDevice As DeviceID_t
            Public calledDevice As DeviceID_t
            Public allocationState As AllocationState_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTAMakePredictiveCallConfEvent_t
            Public newCall As ConnectionID_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTAQueryMwi_t
            Public device As DeviceID_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTAQueryMwiConfEvent_t
            Public messages As Boolean
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTAQueryDnd_t
            Public device As DeviceID_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTAQueryDndConfEvent_t
            Public doNotDisturb As Boolean
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTAQueryFwd_t
            Public device As DeviceID_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTAQueryFwdConfEvent_t
            Public forward As ListForwardParameters_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTAQueryAgentState_t
            Public device As DeviceID_t
        End Structure

        Public Enum AgentState_t
            AG_NOT_READY = 0
            AG_NULL = 1
            AG_READY = 2
            AG_WORK_NOT_READY = 3
            AG_WORK_READY = 4
        End Enum

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTAQueryAgentStateConfEvent_t
            Public agentState As AgentState_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTAQueryLastNumber_t
            Public device As DeviceID_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTAQueryLastNumberConfEvent_t
            Public lastNumber As DeviceID_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTAQueryDeviceInfo_t
            Public device As DeviceID_t
        End Structure

        Public Enum DeviceType_t
            DT_STATION = 0
            DT_LINE = 1
            DT_BUTTON = 2
            DT_ACD = 3
            DT_TRUNK = 4
            DT_OPERATOR = 5
            DT_STATION_GROUP = 16
            DT_LINE_GROUP = 17
            DT_BUTTON_GROUP = 18
            DT_ACD_GROUP = 19
            DT_TRUNK_GROUP = 20
            DT_OPERATOR_GROUP = 21
            DT_OTHER = 255
        End Enum

        Public Const DC_VOICE As Integer = 128
        Public Const DC_DATA As Integer = 64
        Public Const DC_IMAGE As Integer = 32
        Public Const DC_OTHER As Integer = 16

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTAQueryDeviceInfoConfEvent_t
            Public device As DeviceID_t
            Public deviceType As DeviceType_t
            Public deviceClass As Byte
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTAReconnectCall_t
            Public activeCall As ConnectionID_t
            Public heldCall As ConnectionID_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTAReconnectCallConfEvent_t
            Public nil As Nulltype
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTARetrieveCall_t
            Public heldCall As ConnectionID_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTARetrieveCallConfEvent_t
            Public nil As Nulltype
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTASetMwi_t
            Public device As DeviceID_t
            Public messages As Boolean
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTASetMwiConfEvent_t
            Public nil As Nulltype
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTASetDnd_t
            Public device As DeviceID_t
            Public doNotDisturb As Boolean
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTASetDndConfEvent_t
            Public nil As Nulltype
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTASetFwd_t
            Public device As DeviceID_t
            Public forward As ForwardingInfo_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTASetFwdConfEvent_t
            Public nil As Nulltype
        End Structure

        Public Enum AgentMode_t
            AM_LOG_IN = 0
            AM_LOG_OUT = 1
            AM_NOT_READY = 2
            AM_READY = 3
            AM_WORK_NOT_READY = 4
            AM_WORK_READY = 5
        End Enum

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTASetAgentState_t
            Public device As DeviceID_t
            Public agentMode As AgentMode_t
            Public agentID As AgentID_t
            Public agentGroup As DeviceID_t
            Public agentPassword As AgentPassword_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTASetAgentStateConfEvent_t
            Public nil As Nulltype
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTATransferCall_t
            Public heldCall As ConnectionID_t
            Public activeCall As ConnectionID_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTATransferCallConfEvent_t
            Public newCall As ConnectionID_t
            Public connList As ConnectionList_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTAUniversalFailureConfEvent_t
            Public [error] As CSTAUniversalFailure_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTACallClearedEvent_t
            Public clearedCall As ConnectionID_t
            Public localConnectionInfo As LocalConnectionState_t
            Public cause As CSTAEventCause_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTAConferencedEvent_t
            Public primaryOldCall As ConnectionID_t
            Public secondaryOldCall As ConnectionID_t
            Public confController As ExtendedDeviceID_t
            Public addedParty As ExtendedDeviceID_t
            Public conferenceConnections As ConnectionList_t
            Public localConnectionInfo As LocalConnectionState_t
            Public cause As CSTAEventCause_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTAConnectionClearedEvent_t
            Public droppedConnection As ConnectionID_t
            Public releasingDevice As ExtendedDeviceID_t
            Public localConnectionInfo As LocalConnectionState_t
            Public cause As CSTAEventCause_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTADeliveredEvent_t
            Public connection As ConnectionID_t
            Public alertingDevice As ExtendedDeviceID_t
            Public callingDevice As ExtendedDeviceID_t
            Public calledDevice As ExtendedDeviceID_t
            Public lastRedirectionDevice As ExtendedDeviceID_t
            Public localConnectionInfo As LocalConnectionState_t
            Public cause As CSTAEventCause_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTADivertedEvent_t
            Public connection As ConnectionID_t
            Public divertingDevice As ExtendedDeviceID_t
            Public newDestination As ExtendedDeviceID_t
            Public localConnectionInfo As LocalConnectionState_t
            Public cause As CSTAEventCause_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTAEstablishedEvent_t
            Public establishedConnection As ConnectionID_t
            Public answeringDevice As ExtendedDeviceID_t
            Public callingDevice As ExtendedDeviceID_t
            Public calledDevice As ExtendedDeviceID_t
            Public lastRedirectionDevice As ExtendedDeviceID_t
            Public localConnectionInfo As LocalConnectionState_t
            Public cause As CSTAEventCause_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTAFailedEvent_t
            Public failedConnection As ConnectionID_t
            Public failingDevice As ExtendedDeviceID_t
            Public calledDevice As ExtendedDeviceID_t
            Public localConnectionInfo As LocalConnectionState_t
            Public cause As CSTAEventCause_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTAHeldEvent_t
            Public heldConnection As ConnectionID_t
            Public holdingDevice As ExtendedDeviceID_t
            Public localConnectionInfo As LocalConnectionState_t
            Public cause As CSTAEventCause_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTANetworkReachedEvent_t
            Public connection As ConnectionID_t
            Public trunkUsed As ExtendedDeviceID_t
            Public calledDevice As ExtendedDeviceID_t
            Public localConnectionInfo As LocalConnectionState_t
            Public cause As CSTAEventCause_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTAOriginatedEvent_t
            Public originatedConnection As ConnectionID_t
            Public callingDevice As ExtendedDeviceID_t
            Public calledDevice As ExtendedDeviceID_t
            Public localConnectionInfo As LocalConnectionState_t
            Public cause As CSTAEventCause_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTAQueuedEvent_t
            Public queuedConnection As ConnectionID_t
            Public queue As ExtendedDeviceID_t
            Public callingDevice As ExtendedDeviceID_t
            Public calledDevice As ExtendedDeviceID_t
            Public lastRedirectionDevice As ExtendedDeviceID_t
            Public numberQueued As Short
            Public localConnectionInfo As LocalConnectionState_t
            Public cause As CSTAEventCause_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTARetrievedEvent_t
            Public retrievedConnection As ConnectionID_t
            Public retrievingDevice As ExtendedDeviceID_t
            Public localConnectionInfo As LocalConnectionState_t
            Public cause As CSTAEventCause_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTAServiceInitiatedEvent_t
            Public initiatedConnection As ConnectionID_t
            Public localConnectionInfo As LocalConnectionState_t
            Public cause As CSTAEventCause_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTATransferredEvent_t
            Public primaryOldCall As ConnectionID_t
            Public secondaryOldCall As ConnectionID_t
            Public transferringDevice As ExtendedDeviceID_t
            Public transferredDevice As ExtendedDeviceID_t
            Public transferredConnections As ConnectionList_t
            Public localConnectionInfo As LocalConnectionState_t
            Public cause As CSTAEventCause_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTACallInformationEvent_t
            Public connection As ConnectionID_t
            Public device As ExtendedDeviceID_t
            Public accountInfo As AccountInfo_t
            Public authorisationCode As AuthCode_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTADoNotDisturbEvent_t
            Public device As ExtendedDeviceID_t
            Public doNotDisturbOn As Boolean
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTAForwardingEvent_t
            Public device As ExtendedDeviceID_t
            Public forwardingInformation As ForwardingInfo_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTAMessageWaitingEvent_t
            Public deviceForMessage As ExtendedDeviceID_t
            Public invokingDevice As ExtendedDeviceID_t
            Public messageWaitingOn As Boolean
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTALoggedOnEvent_t
            Public agentDevice As ExtendedDeviceID_t
            Public agentID As AgentID_t
            Public agentGroup As DeviceID_t
            Public password As AgentPassword_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTALoggedOffEvent_t
            Public agentDevice As ExtendedDeviceID_t
            Public agentID As AgentID_t
            Public agentGroup As DeviceID_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTANotReadyEvent_t
            Public agentDevice As ExtendedDeviceID_t
            Public agentID As AgentID_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTAReadyEvent_t
            Public agentDevice As ExtendedDeviceID_t
            Public agentID As AgentID_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTAWorkNotReadyEvent_t
            Public agentDevice As ExtendedDeviceID_t
            Public agentID As AgentID_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTAWorkReadyEvent_t
            Public agentDevice As ExtendedDeviceID_t
            Public agentID As AgentID_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTARouteRegisterReq_t
            Public routingDevice As DeviceID_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTARouteRegisterReqConfEvent_t
            Public registerReqID As Integer
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTARouteRegisterCancel_t
            Public routeRegisterReqID As Integer
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTARouteRegisterCancelConfEvent_t
            Public routeRegisterReqID As Integer
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTARouteRegisterAbortEvent_t
            Public routeRegisterReqID As Integer
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTARouteRequestEvent_t
            Public routeRegisterReqID As Integer
            Public routingCrossRefID As Integer
            Public currentRoute As DeviceID_t
            Public callingDevice As DeviceID_t
            Public routedCall As ConnectionID_t
            Public routedSelAlgorithm As SelectValue_t
            Public priority As Boolean
            Public setupInformation As SetUpValues_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTARouteSelectRequest_t
            Public routeRegisterReqID As Integer
            Public routingCrossRefID As Integer
            Public routeSelected As DeviceID_t
            Public remainRetry As Short
            Public setupInformation As SetUpValues_t
            Public routeUsedReq As Boolean
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTAReRouteRequest_t
            Public routeRegisterReqID As Integer
            Public routingCrossRefID As Integer
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTARouteUsedEvent_t
            Public routeRegisterReqID As Integer
            Public routingCrossRefID As Integer
            Public routeUsed As DeviceID_t
            Public callingDevice As DeviceID_t
            Public domain As Boolean
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTARouteEndEvent_t
            Public routeRegisterReqID As Integer
            Public routingCrossRefID As Integer
            Public errorValue As CSTAUniversalFailure_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTARouteEndRequest_t
            Public routeRegisterReqID As Integer
            Public routingCrossRefID As Integer
            Public errorValue As CSTAUniversalFailure_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTAEscapeSvc_t
            Public nil As Nulltype
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTAEscapeSvcConfEvent_t
            Public nil As Nulltype
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTAEscapeSvcReqEvent_t
            Public nil As Nulltype
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTAEscapeSvcReqConf_t
            Public errorValue As CSTAUniversalFailure_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTAPrivateEvent_t
            Public nil As Nulltype
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTAPrivateStatusEvent_t
            Public nil As Nulltype
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTASendPrivateEvent_t
            Public nil As Nulltype
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTABackInServiceEvent_t
            Public device As DeviceID_t
            Public cause As CSTAEventCause_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTAOutOfServiceEvent_t
            Public device As DeviceID_t
            Public cause As CSTAEventCause_t
        End Structure

        Public Enum SystemStatus_t
            SS_INITIALIZING = 0
            SS_ENABLED = 1
            SS_NORMAL = 2
            SS_MESSAGES_LOST = 3
            SS_DISABLED = 4
            SS_OVERLOAD_IMMINENT = 5
            SS_OVERLOAD_REACHED = 6
            SS_OVERLOAD_RELIEVED = 7
        End Enum

        Public Const SF_INITIALIZING As Integer = 128
        Public Const SF_ENABLED As Integer = 64
        Public Const SF_NORMAL As Integer = 32
        Public Const SF_MESSAGES_LOST As Integer = 16
        Public Const SF_DISABLED As Integer = 8
        Public Const SF_OVERLOAD_IMMINENT As Integer = 4
        Public Const SF_OVERLOAD_REACHED As Integer = 2
        Public Const SF_OVERLOAD_RELIEVED As Integer = 1

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTAReqSysStat_t
            Public nil As Nulltype
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTASysStatReqConfEvent_t
            Public systemStatus As SystemStatus_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTASysStatStart_t
            Public statusFilter As Byte
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTASysStatStartConfEvent_t
            Public statusFilter As Byte
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTASysStatStop_t
            Public nil As Nulltype
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTASysStatStopConfEvent_t
            Public nil As Nulltype
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTAChangeSysStatFilter_t
            Public statusFilter As Byte
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTAChangeSysStatFilterConfEvent_t
            Public statusFilterSelected As Byte
            Public statusFilterActive As Byte
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTASysStatEvent_t
            Public systemStatus As SystemStatus_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTASysStatEndedEvent_t
            Public nil As Nulltype
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTASysStatReqEvent_t
            Public nil As Nulltype
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTAReqSysStatConf_t
            Public systemStatus As SystemStatus_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTASysStatEventSend_t
            Public systemStatus As SystemStatus_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTAMonitorDevice_t
            Public deviceID As DeviceID_t
            Public monitorFilter As CSTAMonitorFilter_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTAMonitorCall_t
            Public [call] As ConnectionID_t
            Public monitorFilter As CSTAMonitorFilter_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTAMonitorCallsViaDevice_t
            Public deviceID As DeviceID_t
            Public monitorFilter As CSTAMonitorFilter_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTAMonitorConfEvent_t
            Public monitorCrossRefID As Integer
            Public monitorFilter As CSTAMonitorFilter_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTAChangeMonitorFilter_t
            Public monitorCrossRefID As Integer
            Public monitorFilter As CSTAMonitorFilter_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTAChangeMonitorFilterConfEvent_t
            Public monitorFilter As CSTAMonitorFilter_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTAMonitorStop_t
            Public monitorCrossRefID As Integer
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTAMonitorStopConfEvent_t
            Public nil As Nulltype
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTAMonitorEndedEvent_t
            Public cause As CSTAEventCause_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTASnapshotCall_t
            Public snapshotObject As ConnectionID_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTASnapshotCallConfEvent_t
            Public snapshotData As CSTASnapshotCallData_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTASnapshotDevice_t
            Public snapshotObject As DeviceID_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTASnapshotDeviceConfEvent_t
            Public snapshotData As CSTASnapshotDeviceData_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTAGetAPICaps_t
            Public nil As Nulltype
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTAGetAPICapsConfEvent_t
            Public alternateCall As Short
            Public answerCall As Short
            Public callCompletion As Short
            Public clearCall As Short
            Public clearConnection As Short
            Public conferenceCall As Short
            Public consultationCall As Short
            Public deflectCall As Short
            Public pickupCall As Short
            Public groupPickupCall As Short
            Public holdCall As Short
            Public makeCall As Short
            Public makePredictiveCall As Short
            Public queryMwi As Short
            Public queryDnd As Short
            Public queryFwd As Short
            Public queryAgentState As Short
            Public queryLastNumber As Short
            Public queryDeviceInfo As Short
            Public reconnectCall As Short
            Public retrieveCall As Short
            Public setMwi As Short
            Public setDnd As Short
            Public setFwd As Short
            Public setAgentState As Short
            Public transferCall As Short
            Public eventReport As Short
            Public callClearedEvent As Short
            Public conferencedEvent As Short
            Public connectionClearedEvent As Short
            Public deliveredEvent As Short
            Public divertedEvent As Short
            Public establishedEvent As Short
            Public failedEvent As Short
            Public heldEvent As Short
            Public networkReachedEvent As Short
            Public originatedEvent As Short
            Public queuedEvent As Short
            Public retrievedEvent As Short
            Public serviceInitiatedEvent As Short
            Public transferredEvent As Short
            Public callInformationEvent As Short
            Public doNotDisturbEvent As Short
            Public forwardingEvent As Short
            Public messageWaitingEvent As Short
            Public loggedOnEvent As Short
            Public loggedOffEvent As Short
            Public notReadyEvent As Short
            Public readyEvent As Short
            Public workNotReadyEvent As Short
            Public workReadyEvent As Short
            Public backInServiceEvent As Short
            Public outOfServiceEvent As Short
            Public privateEvent As Short
            Public routeRequestEvent As Short
            Public reRoute As Short
            Public routeSelect As Short
            Public routeUsedEvent As Short
            Public routeEndEvent As Short
            Public monitorDevice As Short
            Public monitorCall As Short
            Public monitorCallsViaDevice As Short
            Public changeMonitorFilter As Short
            Public monitorStop As Short
            Public monitorEnded As Short
            Public snapshotDeviceReq As Short
            Public snapshotCallReq As Short
            Public escapeService As Short
            Public privateStatusEvent As Short
            Public escapeServiceEvent As Short
            Public escapeServiceConf As Short
            Public sendPrivateEvent As Short
            Public sysStatReq As Short
            Public sysStatStart As Short
            Public sysStatStop As Short
            Public changeSysStatFilter As Short
            Public sysStatReqEvent As Short
            Public sysStatReqConf As Short
            Public sysStatEvent As Short
        End Structure

        Public Enum CSTALevel_t
            CSTA_HOME_WORK_TOP = 1
            CSTA_AWAY_WORK_TOP = 2
            CSTA_DEVICE_DEVICE_MONITOR = 3
            CSTA_CALL_DEVICE_MONITOR = 4
            CSTA_CALL_CONTROL = 5
            CSTA_ROUTING = 6
            CSTA_CALL_CALL_MONITOR = 7
        End Enum

        Public Enum SDBLevel_t
            NO_SDB_CHECKING = -1
            ACS_ONLY = 1
            ACS_AND_CSTA_CHECKING = 0
        End Enum

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTAGetDeviceList_t
            Public index As Integer
            Public level As CSTALevel_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure DeviceList_t
            Public count As Short
            <MarshalAs(UnmanagedType.ByValArray, SizeConst:=20)> _
            Public device As DeviceID_t()
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTAGetDeviceListConfEvent_t
            Public driverSdbLevel As SDBLevel_t
            Public level As CSTALevel_t
            Public index As Integer
            Public devList As DeviceList_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTAQueryCallMonitor_t
            Public nil As Nulltype
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTAQueryCallMonitorConfEvent_t
            Public callMonitor As Boolean
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTARouteRequestExtEvent_t
            Public routeRegisterReqID As Integer
            Public routingCrossRefID As Integer
            Public currentRoute As ExtendedDeviceID_t
            Public callingDevice As ExtendedDeviceID_t
            Public routedCall As ConnectionID_t
            Public routedSelAlgorithm As SelectValue_t
            Public priority As Boolean
            Public setupInformation As SetUpValues_t
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure CSTARouteUsedExtEvent_t
            Public routeRegisterReqID As Integer
            Public routingCrossRefID As Integer
            Public routeUsed As ExtendedDeviceID_t
            Public callingDevice As ExtendedDeviceID_t
            Public domain As Boolean
        End Structure

        ' Generated by PInvoke Wizard (v 1.3) from The Paul Yao Company http://www.paulyao.com 

        Public Const TSERV_SAP_CSTA As Integer = 1369
        Public Const CLIENT_SAP_CSTA As Integer = 22789
        Public Const TSERV_SAP_NMSRV As Integer = 1371
        Public Const CLIENT_SAP_NMSRV As Integer = 23301
        Public Const ACSPOSITIVE_ACK As Integer = 0
        Public Const ACSERR_APIVERDENIED As Integer = -1
        Public Const ACSERR_BADPARAMETER As Integer = -2
        Public Const ACSERR_DUPSTREAM As Integer = -3
        Public Const ACSERR_NODRIVER As Integer = -4
        Public Const ACSERR_NOSERVER As Integer = -5
        Public Const ACSERR_NORESOURCE As Integer = -6
        Public Const ACSERR_UBUFSMALL As Integer = -7
        Public Const ACSERR_NOMESSAGE As Integer = -8
        Public Const ACSERR_UNKNOWN As Integer = -9
        Public Const ACSERR_BADHDL As Integer = -10
        Public Const ACSERR_STREAM_FAILED As Integer = -11
        Public Const ACSERR_NOBUFFERS As Integer = -12
        Public Const ACSERR_QUEUE_FULL As Integer = -13
        Public Enum InvokeIDType_t
            APP_GEN_ID
            LIB_GEN_ID
        End Enum

        Public Const ACSREQUEST As Integer = 0
        Public Const ACSUNSOLICITED As Integer = 1
        Public Const ACSCONFIRMATION As Integer = 2
        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure ACSEventHeader_t
            Public acsHandle As ULong
            Public eventClass As UShort
            Public eventType As UShort
        End Structure

        ' *****************************
        ' * Nested elements not supported
        ' *****************************
        ' typedef struct 
        ' {
        ' 	union 
        ' 	{
        ' 		ACSUniversalFailureEvent_t	failureEvent;
        ' 	} u;
        ' } ACSUnsolicitedEvent;
        ' *****************************

        ' *****************************
        ' * Nested elements not supported
        ' *****************************
        ' typedef struct 
        ' {
        ' 	InvokeID_t	invokeID;
        ' 	union 
        ' 	{
        ' 		ACSOpenStreamConfEvent_t		acsopen;
        ' 		ACSCloseStreamConfEvent_t		acsclose;
        ' 		ACSUniversalFailureConfEvent_t	failureEvent;
        ' 	} u;
        ' } ACSConfirmationEvent;
        ' *****************************

        Public Const ACS_MAX_HEAP As Integer = 1024
        ' *****************************
        ' * Nested elements not supported
        ' *****************************
        ' typedef struct 
        ' {
        ' 	ACSEventHeader_t	eventHeader;
        ' 	union 
        ' 	{
        ' 		ACSUnsolicitedEvent		acsUnsolicited;
        ' 		ACSConfirmationEvent	acsConfirmation;
        ' 	} event;
        ' 	char	heap[ACS_MAX_HEAP];
        ' } ACSEvent_t;
        ' *****************************

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Public Structure PrivateData_t
            <MarshalAs(UnmanagedType.ByValArray, SizeConst:=32)> _
            Public vendor As Char()
            Public length As UShort
            <MarshalAs(UnmanagedType.ByValArray, SizeConst:=1)> _
            Public data As Char()
        End Structure

        Public Structure EventBuf_t
            <MarshalAs(UnmanagedType.ByValArray, SizeConst:=CSTA_MAX_HEAP)> _
            Public data As Byte()
        End Structure

        Public Const PRIVATE_DATA_ENCODING As Integer = 0
        <DllImport("csta32.dll")> _
        Public Shared Function acsOpenStream(ByRef acsHandle As UInt32, ByVal invokeIDType As Integer, ByVal invokeID As UInt32, ByVal streamType As Integer, ByVal serverID As Char(), ByVal loginID As Char(), _
         ByVal passwd As Char(), ByVal applicationName As Char(), ByVal acsLevelReq As Integer, ByVal apiVer As Char(), ByVal sendQSize As UShort, ByVal sendExtraBufs As UShort, _
         ByVal recvQSize As UShort, ByVal recvExtraBufs As UShort, ByRef priv As PrivateData_t) As Integer
        End Function

        ' public static extern int acsCloseStream (ACSHandle_t acsHandle, uint invokeID, ref PrivateData_t priv);
        <DllImport("csta32.dll")> _
        Public Shared Function acsCloseStream(ByVal acsHandle As UInt32, ByVal invokeID As UInt32, ByRef priv As PrivateData_t) As Integer
        End Function

        ' public static extern int acsAbortStream (ACSHandle_t acsHandle, ref PrivateData_t priv);
        <DllImport("csta32.dll")> _
        Public Shared Function acsAbortStream(ByVal acsHandle As UInt32, ByRef priv As PrivateData_t) As Integer
        End Function

        ' public static extern int acsFlushEventQueue (ACSHandle_t acsHandle);
        <DllImport("csta32.dll")> _
        Public Shared Function acsFlushEventQueue(ByVal acsHandle As UInt32) As Integer
        End Function

        <DllImport("csta32.dll")> _
        Public Shared Function acsGetEventPoll(ByVal acsHandle As UInt32, ByRef eventBuf As EventBuf_t, ByRef eventBufSize As UShort, ByRef privData As PrivateData_t, ByRef numEvents As UShort) As Integer
        End Function

        ' public static extern int acsGetEventBlock (ACSHandle_t acsHandle, ref EventBuf_t eventBuf, ref ushort eventBufSize, ref PrivateData_t privData, ref ushort numEvents);
        <DllImport("csta32.dll")> _
        Public Shared Function acsGetEventBlock(ByVal acsHandle As UInt32, ByRef eventBuf As EventBuf_t, ByRef eventBufSize As UShort, ByRef privData As PrivateData_t, ByRef numEvents As UShort) As Integer
        End Function

        ' public static extern int acsEventNotify (ACSHandle_t acsHandle, IntPtr hwnd, int msg, Boolean notifyAll);
        <DllImport("csta32.dll")> _
        Public Shared Function acsEventNotify(ByVal acsHandle As UInt32, ByVal hwnd As IntPtr, ByVal msg As Integer, ByVal notifyAll As Boolean) As Integer
        End Function

        ' public static extern int acsSetESR (ACSHandle_t acsHandle, EventBuf_t param1);
        <DllImport("csta32.dll")> _
        Public Shared Function acsSetESR(ByVal acsHandle As UInt32, ByVal param1 As EventBuf_t) As Integer
        End Function

        ' [DllImport("csta32.dll")]
        ' public static extern int acsEventNotify (ACSHandle_t acsHandle, IntPtr hwnd, int msg, Boolean notifyAll);
        ' public static extern int acsEventNotify(ref IntPtr acsHandle, IntPtr hwnd, int msg, Boolean notifyAll);

        ' [DllImport("csta32.dll")]
        ' public static extern int acsSetESR (ACSHandle_t acsHandle, void param1);

        ' [DllImport("csta32.dll")]
        ' public static extern int acsSetESR (ACSHandle_t acsHandle, void param1);

        ' public static extern int acsEventNotify (ACSHandle_t acsHandle, IntPtr hwnd, int msg, byte notifyAll);
        <DllImport("csta32.dll")> _
        Public Shared Function acsEventNotify(ByVal acsHandle As UInt32, ByVal hwnd As IntPtr, ByVal msg As Integer, ByVal notifyAll As Byte) As Integer
        End Function

        ' [DllImport("csta32.dll")]
        ' public static extern int acsSetESR (ACSHandle_t acsHandle, void param1);

        ' public const int kTSAPIEventClass = 'Csta';
        ' public const int kTSAPIEventArrived = 'NuEv';
        ' public const int keyTSAPIEventClass = 'clas';
        ' public const int keyTSAPIEventType = 'type';
        ' public const int keyStreamHandle = 'strm';

        ' [DllImport("csta32.dll")]
        ' public static extern int acsEventNotify (ACSHandle_t acsHandle, ref AEAddressDesc targetAddr, Boolean notifyAll);
        ' public static extern int acsEventNotify(ref IntPtr acsHandle, ref AEAddressDesc targetAddr, Boolean notifyAll);

        ' [DllImport("csta32.dll")]
        ' public static extern int acsSetESR (ACSHandle_t acsHandle, UniversalProcPtr esr, uint esrParam, Boolean notifyAll);
        ' public static extern int acsSetESR(ref IntPtr acsHandle, UniversalProcPtr esr, uint esrParam, Boolean notifyAll);

        ' public const int kTSAPIEventClass = 'Csta';
        ' public const int kTSAPIEventArrived = 'NuEv';
        ' public const int keyTSAPIEventClass = 'clas';
        ' public const int keyTSAPIEventType = 'type';
        ' public const int keyStreamHandle = 'strm';

        ' [DllImport("csta32.dll")]
        ' public static extern int acsEventNotify (ACSHandle_t acsHandle, ref AEAddressDesc targetAddr, Boolean notifyAll);

        ' [DllImport("csta32.dll")]
        ' public static extern uppEsrFuncProcInfo RESULT_SIZE (SIZE_CODE param0);

        ' [DllImport("csta32.dll")]
        ' public static extern int acsSetESR (ACSHandle_t acsHandle, UniversalProcPtr esr, uint esrParam, Boolean notifyAll);

        ' [DllImport("csta32.dll")]
        ' public static extern uppEnumServerNamesProcInfo RESULT_SIZE (SIZE_CODE param0);

        <DllImport("csta32.dll")> _
        Public Shared Function acsGetFile(ByVal acsHandle As UInt32) As Integer
        End Function

        ' [DllImport("csta32.dll")]
        ' public static extern int acsGetFile (ACSHandle_t acsHandle);

        ' [DllImport("csta32.dll")]
        ' public static extern int acsEnumServerNames (StreamType_t streamType, UniversalProcPtr userCB, uint lParam);

        ' [DllImport("csta32.dll")]
        ' public static extern int acsQueryAuthInfo (ref ServerID_t serverID, ref ACSAuthInfo_t authInfo);

        Public Const CSTA_API_VERSION As String = "TS2"
        Public Const CSTAREQUEST As Integer = 3
        Public Const CSTAUNSOLICITED As Integer = 4
        Public Const CSTACONFIRMATION As Integer = 5
        Public Const CSTAEVENTREPORT As Integer = 6
        Public Const CSTA_MAX_GET_DEVICE As Integer = 20
        ' *****************************
        ' * Nested elements not supported
        ' *****************************
        ' typedef struct 
        ' {
        ' 	InvokeID_t	invokeID;
        ' 	union 
        ' 	{
        ' 		CSTARouteRequestEvent_t		routeRequest;
        ' 		CSTARouteRequestExtEvent_t	routeRequestExt;
        ' 		CSTAReRouteRequest_t		reRouteRequest;
        ' 		CSTAEscapeSvcReqEvent_t		escapeSvcReqeust;
        ' 		CSTASysStatReqEvent_t		sysStatRequest;
        ' 	} u;
        ' } CSTARequestEvent;
        ' *****************************

        ' *****************************
        ' * Nested elements not supported
        ' *****************************
        ' typedef struct
        ' {
        ' 	union
        ' 	{
        ' 		CSTARouteRegisterAbortEvent_t   registerAbort;
        ' 		CSTARouteUsedEvent_t			routeUsed;
        ' 		CSTARouteUsedExtEvent_t			routeUsedExt;
        ' 		CSTARouteEndEvent_t				routeEnd;
        ' 		CSTAPrivateEvent_t				privateEvent;
        ' 		CSTASysStatEvent_t				sysStat;
        ' 		CSTASysStatEndedEvent_t			sysStatEnded;
        ' 	}u;
        ' } CSTAEventReport;
        ' *****************************

        ' *****************************
        ' * Nested elements not supported
        ' *****************************
        ' typedef struct 
        ' {
        ' 	CSTAMonitorCrossRefID_t		monitorCrossRefId;
        ' 	union 
        ' 	{
        ' 		CSTACallClearedEvent_t			callCleared;
        ' 		CSTAConferencedEvent_t			conferenced;
        ' 		CSTAConnectionClearedEvent_t	connectionCleared;
        ' 		CSTADeliveredEvent_t			delivered;
        ' 		CSTADivertedEvent_t				diverted;
        ' 		CSTAEstablishedEvent_t			established;
        ' 		CSTAFailedEvent_t				failed;
        ' 		CSTAHeldEvent_t					held;
        ' 		CSTANetworkReachedEvent_t		networkReached;
        ' 		CSTAOriginatedEvent_t			originated;
        ' 		CSTAQueuedEvent_t				queued;
        ' 		CSTARetrievedEvent_t			retrieved;
        ' 		CSTAServiceInitiatedEvent_t		serviceInitiated;
        ' 		CSTATransferredEvent_t			transferred;
        ' 		CSTACallInformationEvent_t		callInformation;
        ' 		CSTADoNotDisturbEvent_t			doNotDisturb;
        ' 		CSTAForwardingEvent_t			forwarding;
        ' 		CSTAMessageWaitingEvent_t		messageWaiting;
        ' 		CSTALoggedOnEvent_t				loggedOn;
        ' 		CSTALoggedOffEvent_t			loggedOff;
        ' 		CSTANotReadyEvent_t				notReady;
        ' 		CSTAReadyEvent_t				ready;
        ' 		CSTAWorkNotReadyEvent_t			workNotReady;
        ' 		CSTAWorkReadyEvent_t			workReady;
        ' 		CSTABackInServiceEvent_t		backInService;
        ' 		CSTAOutOfServiceEvent_t			outOfService;
        ' 		CSTAPrivateStatusEvent_t		privateStatus;
        ' 		CSTAMonitorEndedEvent_t  		monitorEnded;
        ' 	} u;
        ' } CSTAUnsolicitedEvent;
        ' *****************************

        ' *****************************
        ' * Nested elements not supported
        ' *****************************
        ' typedef struct 
        ' {
        ' 	InvokeID_t	invokeID;
        ' 	union 
        ' 	{
        ' 		CSTAAlternateCallConfEvent_t		alternateCall;
        ' 		CSTAAnswerCallConfEvent_t			answerCall;
        ' 		CSTACallCompletionConfEvent_t		callCompletion;
        ' 		CSTAClearCallConfEvent_t			clearCall;
        ' 		CSTAClearConnectionConfEvent_t    	clearConnection;
        ' 		CSTAConferenceCallConfEvent_t		conferenceCall;
        ' 		CSTAConsultationCallConfEvent_t		consultationCall;
        ' 		CSTADeflectCallConfEvent_t			deflectCall;
        ' 		CSTAPickupCallConfEvent_t			pickupCall;
        ' 		CSTAGroupPickupCallConfEvent_t		groupPickupCall;
        ' 		CSTAHoldCallConfEvent_t				holdCall;
        ' 		CSTAMakeCallConfEvent_t				makeCall;
        ' 		CSTAMakePredictiveCallConfEvent_t 	makePredictiveCall;
        ' 		CSTAQueryMwiConfEvent_t				queryMwi;
        ' 		CSTAQueryDndConfEvent_t				queryDnd;
        ' 		CSTAQueryFwdConfEvent_t				queryFwd;
        ' 		CSTAQueryAgentStateConfEvent_t		queryAgentState;
        ' 		CSTAQueryLastNumberConfEvent_t		queryLastNumber;
        ' 		CSTAQueryDeviceInfoConfEvent_t		queryDeviceInfo;
        ' 		CSTAReconnectCallConfEvent_t		reconnectCall;
        ' 		CSTARetrieveCallConfEvent_t			retrieveCall;
        ' 		CSTASetMwiConfEvent_t				setMwi;
        ' 		CSTASetDndConfEvent_t				setDnd;
        ' 		CSTASetFwdConfEvent_t				setFwd;
        ' 		CSTASetAgentStateConfEvent_t		setAgentState;
        ' 		CSTATransferCallConfEvent_t			transferCall;
        ' 		CSTAUniversalFailureConfEvent_t		universalFailure;
        ' 		CSTAMonitorConfEvent_t				monitorStart;
        ' 		CSTAChangeMonitorFilterConfEvent_t	changeMonitorFilter;
        ' 		CSTAMonitorStopConfEvent_t			monitorStop;
        ' 		CSTASnapshotDeviceConfEvent_t		snapshotDevice;
        ' 		CSTASnapshotCallConfEvent_t			snapshotCall;
        ' 		CSTARouteRegisterReqConfEvent_t		routeRegister;
        ' 		CSTARouteRegisterCancelConfEvent_t	routeCancel;
        ' 		CSTAEscapeSvcConfEvent_t			escapeService;
        ' 		CSTASysStatReqConfEvent_t			sysStatReq;
        ' 		CSTASysStatStartConfEvent_t			sysStatStart;
        ' 		CSTASysStatStopConfEvent_t			sysStatStop;
        ' 		CSTAChangeSysStatFilterConfEvent_t	changeSysStatFilter;
        ' 		CSTAGetAPICapsConfEvent_t			getAPICaps;
        ' 		CSTAGetDeviceListConfEvent_t		getDeviceList;
        ' 		CSTAQueryCallMonitorConfEvent_t		queryCallMonitor;
        ' 	} u;
        ' } CSTAConfirmationEvent;
        ' *****************************

        Public Const CSTA_MAX_HEAP As Integer = 1024
        ' *****************************
        ' * Nested elements not supported
        ' *****************************
        ' typedef struct 
        ' {
        ' 	ACSEventHeader_t	eventHeader;
        ' 	union 
        ' 	{
        ' 		ACSUnsolicitedEvent		acsUnsolicited;
        ' 		ACSConfirmationEvent	acsConfirmation;
        ' 		CSTARequestEvent		cstaRequest;
        ' 		CSTAUnsolicitedEvent	cstaUnsolicited;
        ' 		CSTAConfirmationEvent	cstaConfirmation;
        ' 		CSTAEventReport			cstaEventReport;
        ' 	} event;
        ' 	char	heap[CSTA_MAX_HEAP];
        ' } CSTAEvent_t;
        ' *****************************

        <DllImport("csta32.dll")> _
        Public Shared Function cstaAlternateCall(ByVal acsHandle As UInt32, ByVal invokeID As UInt32, ByRef activeCall As ConnectionID_t, ByRef otherCall As ConnectionID_t, ByRef privateData As PrivateData_t) As Integer
        End Function

        <DllImport("csta32.dll")> _
        Public Shared Function cstaAnswerCall(ByVal acsHandle As UInt32, ByVal invokeID As UInt32, ByRef alertingCall As ConnectionID_t, ByRef privateData As PrivateData_t) As Integer
        End Function

        <DllImport("csta32.dll")> _
        Public Shared Function cstaCallCompletion(ByVal acsHandle As UInt32, ByVal invokeID As UInt32, ByVal feature As Feature_t, ByRef [call] As ConnectionID_t, ByRef privateData As PrivateData_t) As Integer
        End Function

        <DllImport("csta32.dll")> _
        Public Shared Function cstaClearCall(ByVal acsHandle As UInt32, ByVal invokeID As UInt32, ByRef [call] As ConnectionID_t, ByRef privateData As PrivateData_t) As Integer
        End Function

        <DllImport("csta32.dll")> _
        Public Shared Function cstaClearConnection(ByVal acsHandle As UInt32, ByVal invokeID As UInt32, ByRef [call] As ConnectionID_t, ByRef privateData As PrivateData_t) As Integer
        End Function

        <DllImport("csta32.dll")> _
        Public Shared Function cstaConferenceCall(ByVal acsHandle As UInt32, ByVal invokeID As UInt32, ByRef heldCall As ConnectionID_t, ByRef activeCall As ConnectionID_t, ByRef privateData As PrivateData_t) As Integer
        End Function

        <DllImport("csta32.dll")> _
        Public Shared Function cstaConsultationCall(ByVal acsHandle As UInt32, ByVal invokeID As UInt32, ByRef activeCall As ConnectionID_t, ByVal calledDevice As Char(), ByRef privateData As PrivateData_t) As Integer
        End Function

        <DllImport("csta32.dll")> _
        Public Shared Function cstaDeflectCall(ByVal acsHandle As UInt32, ByVal invokeID As UInt32, ByRef deflectCall As ConnectionID_t, ByVal calledDevice As Char(), ByRef privateData As PrivateData_t) As Integer
        End Function

        <DllImport("csta32.dll")> _
        Public Shared Function cstaGroupPickupCall(ByVal acsHandle As UInt32, ByVal invokeID As UInt32, ByRef deflectCall As ConnectionID_t, ByVal pickupDevice As Char(), ByRef privateData As PrivateData_t) As Integer
        End Function

        <DllImport("csta32.dll")> _
        Public Shared Function cstaHoldCall(ByVal acsHandle As UInt32, ByVal invokeID As UInt32, ByRef activeCall As ConnectionID_t, ByVal reservation As Boolean, ByRef privateData As PrivateData_t) As Integer
        End Function

        <DllImport("csta32.dll")> _
        Public Shared Function cstaMakeCall(ByVal acsHandle As UInt32, ByVal invokeID As UInt32, ByVal callingDevice As Char(), ByVal calledDevice As Char(), ByRef privateData As PrivateData_t) As Integer
        End Function

        <DllImport("csta32.dll")> _
        Public Shared Function cstaMakePredictiveCall(ByVal acsHandle As UInt32, ByVal invokeID As UInt32, ByVal callingDevice As Char(), ByVal calledDevice As Char(), ByVal allocationState As AllocationState_t, ByRef privateData As PrivateData_t) As Integer
        End Function

        <DllImport("csta32.dll")> _
        Public Shared Function cstaPickupCall(ByVal acsHandle As UInt32, ByVal invokeID As UInt32, ByRef deflectCall As ConnectionID_t, ByVal calledDevice As Char(), ByRef privateData As PrivateData_t) As Integer
        End Function

        <DllImport("csta32.dll")> _
        Public Shared Function cstaReconnectCall(ByVal acsHandle As UInt32, ByVal invokeID As UInt32, ByRef activeCall As ConnectionID_t, ByRef heldCall As ConnectionID_t, ByRef privateData As PrivateData_t) As Integer
        End Function

        <DllImport("csta32.dll")> _
        Public Shared Function cstaRetrieveCall(ByVal acsHandle As UInt32, ByVal invokeID As UInt32, ByRef heldCall As ConnectionID_t, ByRef privateData As PrivateData_t) As Integer
        End Function

        <DllImport("csta32.dll")> _
        Public Shared Function cstaTransferCall(ByVal acsHandle As UInt32, ByVal invokeID As UInt32, ByRef heldCall As ConnectionID_t, ByRef activeCall As ConnectionID_t, ByRef privateData As PrivateData_t) As Integer
        End Function

        <DllImport("csta32.dll")> _
        Public Shared Function cstaSetMsgWaitingInd(ByVal acsHandle As UInt32, ByVal invokeID As UInt32, ByVal device As Char(), ByVal messages As Boolean, ByRef privateData As PrivateData_t) As Integer
        End Function

        <DllImport("csta32.dll")> _
        Public Shared Function cstaSetDoNotDisturb(ByVal acsHandle As UInt32, ByVal invokeID As UInt32, ByVal device As Char(), ByVal doNotDisturb As Boolean, ByRef privateData As PrivateData_t) As Integer
        End Function

        <DllImport("csta32.dll")> _
        Public Shared Function cstaSetForwarding(ByVal acsHandle As UInt32, ByVal invokeID As UInt32, ByVal device As Char(), ByVal forwardingType As ForwardingType_t, ByVal forwardingOn As Boolean, ByVal forwardingDestination As Char(), _
         ByRef privateData As PrivateData_t) As Integer
        End Function

        <DllImport("csta32.dll")> _
        Public Shared Function cstaSetAgentState(ByVal acsHandle As UInt32, ByVal invokeID As UInt32, ByVal device As Char(), ByVal agentMode As AgentMode_t, ByVal agentID As Char(), ByVal agentGroup As Char(), _
         ByVal agentPassword As Char(), ByRef privateData As PrivateData_t) As Integer
        End Function

        <DllImport("csta32.dll")> _
        Public Shared Function cstaQueryMsgWaitingInd(ByVal acsHandle As UInt32, ByVal invokeID As UInt32, ByVal device As Char(), ByRef privateData As PrivateData_t) As Integer
        End Function

        <DllImport("csta32.dll")> _
        Public Shared Function cstaQueryDoNotDisturb(ByVal acsHandle As UInt32, ByVal invokeID As UInt32, ByVal device As Char(), ByRef privateData As PrivateData_t) As Integer
        End Function

        <DllImport("csta32.dll")> _
        Public Shared Function cstaQueryForwarding(ByVal acsHandle As UInt32, ByVal invokeID As UInt32, ByVal device As Char(), ByRef privateData As PrivateData_t) As Integer
        End Function

        <DllImport("csta32.dll")> _
        Public Shared Function cstaQueryAgentState(ByVal acsHandle As UInt32, ByVal invokeID As UInt32, ByVal device As Char(), ByRef privateData As PrivateData_t) As Integer
        End Function

        <DllImport("csta32.dll")> _
        Public Shared Function cstaQueryLastNumber(ByVal acsHandle As UInt32, ByVal invokeID As UInt32, ByVal device As Char(), ByRef privateData As PrivateData_t) As Integer
        End Function

        <DllImport("csta32.dll")> _
        Public Shared Function cstaQueryDeviceInfo(ByVal acsHandle As UInt32, ByVal invokeID As UInt32, ByVal device As Char(), ByRef privateData As PrivateData_t) As Integer
        End Function

        <DllImport("csta32.dll")> _
        Public Shared Function cstaMonitorDevice(ByVal acsHandle As UInt32, ByVal invokeID As UInt32, ByVal deviceID As Char(), ByRef monitorFilter As CSTAMonitorFilter_t, ByRef privateData As PrivateData_t) As Integer
        End Function

        <DllImport("csta32.dll")> _
        Public Shared Function cstaMonitorCall(ByVal acsHandle As UInt32, ByVal invokeID As UInt32, ByRef [call] As ConnectionID_t, ByRef monitorFilter As CSTAMonitorFilter_t, ByRef privateData As PrivateData_t) As Integer
        End Function

        <DllImport("csta32.dll")> _
        Public Shared Function cstaMonitorCallsViaDevice(ByVal acsHandle As UInt32, ByVal invokeID As UInt32, ByVal deviceID As Char(), ByRef monitorFilter As CSTAMonitorFilter_t, ByRef privateData As PrivateData_t) As Integer
        End Function

        <DllImport("csta32.dll")> _
        Public Shared Function cstaChangeMonitorFilter(ByVal acsHandle As UInt32, ByVal invokeID As UInt32, ByVal monitorCrossRefID As Integer, ByRef filterlist As CSTAMonitorFilter_t, ByRef privateData As PrivateData_t) As Integer
        End Function

        <DllImport("csta32.dll")> _
        Public Shared Function cstaMonitorStop(ByVal acsHandle As UInt32, ByVal invokeID As UInt32, ByVal monitorCrossRefID As Integer, ByRef privateData As PrivateData_t) As Integer
        End Function

        <DllImport("csta32.dll")> _
        Public Shared Function cstaSnapshotCallReq(ByVal acsHandle As UInt32, ByVal invokeID As UInt32, ByRef snapshotObj As ConnectionID_t, ByRef privateData As PrivateData_t) As Integer
        End Function

        <DllImport("csta32.dll")> _
        Public Shared Function cstaSnapshotDeviceReq(ByVal acsHandle As UInt32, ByVal invokeID As UInt32, ByVal snapshotObj As Char(), ByRef privateData As PrivateData_t) As Integer
        End Function

        <DllImport("csta32.dll")> _
        Public Shared Function cstaRouteRegisterReq(ByVal acsHandle As UInt32, ByVal invokeID As UInt32, ByVal routingDevice As Char(), ByRef privateData As PrivateData_t) As Integer
        End Function

        <DllImport("csta32.dll")> _
        Public Shared Function cstaRouteRegisterCancel(ByVal acsHandle As UInt32, ByVal invokeID As UInt32, ByVal routeRegisterReqID As Integer, ByRef privateData As PrivateData_t) As Integer
        End Function

        <DllImport("csta32.dll")> _
        Public Shared Function cstaRouteSelect(ByVal acsHandle As UInt32, ByVal routeRegisterReqID As Integer, ByVal routingCrossRefID As Integer, ByVal routeSelected As Char(), ByVal remainRetry As Short, ByRef setupInformation As SetUpValues_t, _
         ByVal routeUsedReq As Boolean, ByRef privateData As PrivateData_t) As Integer
        End Function

        <DllImport("csta32.dll")> _
        Public Shared Function cstaRouteEnd(ByVal acsHandle As UInt32, ByVal routeRegisterReqID As Integer, ByVal routingCrossRefID As Integer, ByVal errorValue As CSTAUniversalFailure_t, ByRef privateData As PrivateData_t) As Integer
        End Function

        <DllImport("csta32.dll")> _
        Public Shared Function cstaRouteSelectInv(ByVal acsHandle As UInt32, ByVal invokeID As UInt32, ByVal routeRegisterReqID As Integer, ByVal routingCrossRefID As Integer, ByRef routeSelected As DeviceID_t, ByVal remainRetry As Short, _
         ByRef setupInformation As SetUpValues_t, ByVal routeUsedReq As Boolean, ByRef privateData As PrivateData_t) As Integer
        End Function

        <DllImport("csta32.dll")> _
        Public Shared Function cstaRouteEndInv(ByVal acsHandle As UInt32, ByVal invokeID As UInt32, ByVal routeRegisterReqID As Integer, ByVal routingCrossRefID As Integer, ByVal errorValue As CSTAUniversalFailure_t, ByRef privateData As PrivateData_t) As Integer
        End Function

        <DllImport("csta32.dll")> _
        Public Shared Function cstaEscapeService(ByVal acsHandle As UInt32, ByVal invokeID As UInt32, ByRef privateData As PrivateData_t) As Integer
        End Function

        <DllImport("csta32.dll")> _
        Public Shared Function cstaEscapeServiceConf(ByVal acsHandle As UInt32, ByVal invokeID As UInt32, ByVal [error] As CSTAUniversalFailure_t, ByRef privateData As PrivateData_t) As Integer
        End Function

        <DllImport("csta32.dll")> _
        Public Shared Function cstaSendPrivateEvent(ByVal acsHandle As UInt32, ByRef privateData As PrivateData_t) As Integer
        End Function

        <DllImport("csta32.dll")> _
        Public Shared Function cstaSysStatReq(ByVal acsHandle As UInt32, ByVal invokeID As UInt32, ByRef privateData As PrivateData_t) As Integer
        End Function

        <DllImport("csta32.dll")> _
        Public Shared Function cstaSysStatStart(ByVal acsHandle As UInt32, ByVal invokeID As UInt32, ByVal statusFilter As Byte, ByRef privateData As PrivateData_t) As Integer
        End Function

        <DllImport("csta32.dll")> _
        Public Shared Function cstaSysStatStop(ByVal acsHandle As UInt32, ByVal invokeID As UInt32, ByRef privateData As PrivateData_t) As Integer
        End Function

        <DllImport("csta32.dll")> _
        Public Shared Function cstaChangeSysStatFilter(ByVal acsHandle As UInt32, ByVal invokeID As UInt32, ByVal statusFilter As Byte, ByRef privateData As PrivateData_t) As Integer
        End Function

        <DllImport("csta32.dll")> _
        Public Shared Function cstaSysStatReqConf(ByVal acsHandle As UInt32, ByVal invokeID As UInt32, ByVal systemStatus As SystemStatus_t, ByRef privateData As PrivateData_t) As Integer
        End Function

        <DllImport("csta32.dll")> _
        Public Shared Function cstaSysStatEvent(ByVal acsHandle As UInt32, ByVal systemStatus As SystemStatus_t, ByRef privateData As PrivateData_t) As Integer
        End Function

        <DllImport("csta32.dll")> _
        Public Shared Function cstaGetAPICaps(ByVal acsHandle As UInt32, ByVal invokeID As UInt32) As Integer
        End Function

        <DllImport("csta32.dll")> _
        Public Shared Function cstaGetDeviceList(ByVal acsHandle As UInt32, ByVal invokeID As UInt32, ByVal index As Integer, ByVal level As CSTALevel_t) As Integer
        End Function

        <DllImport("csta32.dll")> _
        Public Shared Function cstaQueryCallMonitor(ByVal acsHandle As UInt32, ByVal invokeID As UInt32) As Integer
        End Function


    End Class
End Namespace
