using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol
{
    public enum USERSTATUS : byte
    {
        NOLOGIN_STATUS = 0,     // 미가입상태
        LOGIN_STATUS,           // 가입한 상태
        GAMESTART_STATUS,       // 알방선택한 후 상태
    }

    public enum ENCODINGFMT : byte
    {
        ASCII = 0,
        UNICODE = 1,
        UTF32 = 4,
        UTF7 = 2,
        UTF8 = 3
    }

    public enum MAKE_SLIP_STEP
    {
        INIT,
        ADD_BET,
        PLACE_BET,
        REFRESH_BET
    }

    public enum PROCESS_RESULT
    {
        SUCCESS,
        PLACE_SUCCESS,
        MOVED,      //Odd or Handicap is changed        
        RE_FIXED,   //Result value is fixed "re"
        SMALL_BALANCE,
        ZERO_MAX_STAKE,
        SUSPENDED,  //market is suspended        
        NO_LOGIN,
        ERROR,
        CRITICAL_SITUATION,
    }
}
