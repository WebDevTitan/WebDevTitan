﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Protocol;
using WebSocketSharp.Server;

namespace Project
{

    public delegate string RunScript(string code);
    public delegate void NoParamFunc();
    public delegate void boolParamFunc(bool Param);
    public delegate Rect GetMonitorPosFunc();
    public delegate Task GetCookieFunc(string domain);
    public delegate Task<string> GetPageSourceFunc();

    public class Global
    {
        private static Global s_instance = null;
        public static Global Instance
        {
            get
            {
                if (s_instance == null)
                    s_instance = new Global();

                return s_instance;
            }
        }

        public static bool IsRestStatus = false;
        public static WebSocketServer socketServer = null;

        public static bool bRun = false;
        public static bool bServerConnect = false;

        public static CookieContainer cookieContainer = null;
        public static double balance;
        public static double TotalBalance;
        public static int accountStatus;


        public static UnibetSessionInfo unibetSessionInfo = null;

        //for pinnacle
        public static List<KeyValuePair<string, string>> pinnacleHeaders = new List<KeyValuePair<string, string>>();

        //for betMGM request querys

        public static NameValueCollection betMGMrequestQueries = new NameValueCollection();

        public static string DomusbetToken = "";

        //for 888Sport getting balance.
        public static List<KeyValuePair<string, string>> unifiedclientHeaders = new List<KeyValuePair<string, string>>();
        public static List<KeyValuePair<string, string>> kambicdnHeaders = new List<KeyValuePair<string, string>>();
        public static object locker_unifiedclientHeaders = new object();

        public static string ProxySessionID = "";

        public static UInt32 Version = 196;
        public static string Bookmaker = "Unknown";
        public static UInt32 PackageID = 0;


        public static BetHeaderInfo BetHeader = new BetHeaderInfo();
        public static NameValueCollection placeBetHeaderCollection = new NameValueCollection();


        public static RunScript OpenUrl = null;
        public static RunScript RunScriptCode = null;
        public static RunScript GetStatusValue = null;
        public static NoParamFunc LoadHomeUrl = null;
        public static boolParamFunc SetMonitorVisible = null;
        public static GetMonitorPosFunc GetMonitorPos = null;
        public static IntPtr ViewerHwnd;
        public static GetCookieFunc GetCookie = null;
        public static NoParamFunc RemoveCookies = null;
        public static NoParamFunc RefreshPage = null;
        public static NoParamFunc RefreshBecauseBet365Notloading = null;
        public static GetPageSourceFunc GetPageSource = null;

        public static ManualResetEventSlim waitResponseEvent = new ManualResetEventSlim();
        public static string strAddBetResult = "";
        public static string strPlaceBetResult = "";
        public static string strRequestUrl = "";

        public static ManualResetEventSlim waitResponseEvent1 = new ManualResetEventSlim();
        public static string strWebResponse1 = "";
        public static string strWebResponse1ReqUrl = "";
        public static ManualResetEventSlim waitResponseEvent2 = new ManualResetEventSlim();
        public static string strWebResponse2 = "";
        public static string strWebResponse2ReqUrl = "";
        public static ManualResetEventSlim waitResponseEvent3 = new ManualResetEventSlim();
        public static string strWebResponse3 = "";
        public static string strWebResponse3ReqUrl = "";
        public static ManualResetEventSlim waitResponseEvent4 = new ManualResetEventSlim();
        public static string strWebResponse4 = "";
        public static string strWebResponse4ReqUrl = "";

        public static string GTM = "";
        public const string GetOpenBetListCommandLine = "var OpenBetList = [];var e, n;if (n = Locator.treeLookup.getReference('OPENBETS')){for (e = 0; e < n.getChildren().length; e++){var OpenBetItr = n.getChildren()[e].data;data = [];var m;for (m = 0; m < n.getChildren()[e].getChildren().length; m++){data.push(n.getChildren()[e].getChildren()[m].data);}OpenBetItr['data'] = data;OpenBetList.push(OpenBetItr);}}return JSON.stringify(OpenBetList);";

        public static string FindSearchScript = "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";
        public static string BetslipAttemptScript = "VLvpRnCAJL6L09RGINxipOQZJNOTgIldVTh5CkN6o1UIlMvaK0I+XqCm+1uoCbPU5zyvdCIfonlXK2S0Td4B8MOLc0fym1pxyjqrrM54K1gNLAKR4Debu2WvRHBx3ZGfttFxzb+Rmy1XT0sA0p9kGLXvN2UUI+jxXY8b1qZQuK0qDjT9u9UI5A3/i9DqKE8DxMOnTvq95vCSD5KR6SSNzTQvZzFbDIvpZrbo4abbfZJP/tH7xMm4vQ9jw/+3ldc0XLV9xESY5gjzojlh/gGpMavlnnHbJDhmi5kn92NsxfU2XHDd+LLZUyW/xekq+Xs83bDuRm7xOG4eQDMkrT2wAj2N5y/8yFI8aC+hQdsfUFAAuZFHqxpSQuJjGKqgT67HBKYa3uukICaqZnA3G8rQ3SHifMUquZihNwFPK3RCrsEw3SX/5PEa24HtH8qTDWouxcukvT0MyKdvgF3tOh8teJwmXzrbeSx+FwCUeB/sWJR1Iq9YItAxQaE6T+xgh3hWIUEd3uNfs+I/Qd4gSq9RU6xq/YyCk7Un6NhL3CBb011dMPXFH8uDmvuPbIpDkzm1G2PzESFx6l7Ggk1iPGhtQgtQsvHFdyaqghNAVE4B/H6ga54Lr6KCc7LdE64mPnEbKyD2JLgMgR/njKEaoF+CcWA5OC4cGFnX7jJtyCYeLru9Q1WtWEXnpSxpGkFYTt/qvzSuosfCgiwxZfvkt83oC1aXvR+4Z6lfIoRBhDaDBh1546EPG2xus1kU/cAfRS4EUOmJSuKgAfS0hhepbQpNgO8vr/zJfdX4+yjaN/APqfhzv3GzFduiE6KlDv8owa5qjy3SwxEmVz7DSmVQlJTfSxSWOGgk5CEUSvCz6fEIA3P4C5T53BwrFnXqkObIalySAMg2aUESwQoIu+sqa7ejBeMR8FX3gB97QhqFBa+b0Hw5rsS55/F0y5vQY5KlvDtaUfo3K/Ager8rJBiC2EVmsCAJf0DOgKI1uIUiZv6CTQXbIq+BPNGWZtpNgD4c2JAp7yUDsfRZIJA2eBPi/24K6qkvNys6DKIm8giQ1JMyeKQQwsCIbf3o9U4jLhz3w0W342nluwvVC1DeMfCSy5NziPHZ5sBDD7YRGyZWhaQAu2FjUhG+f8wiBEzGSrROnxpTcvVofUTa9Yy8LbASird2D7yqfq4PKM238bobnS+L4GjU9xraQZEvIcYZNvcZsQqaXgBiLzYQl0r1AGJaKt5DTuo4B4X8v0FXahjhdt/fisWXz/Aax88ApYknkutRNFufLhbIhMrjaWrMgyUJRq0F9aglQlzO8cjxOjYDaXr0lLASVyghLx9DC3bPxBJBHLTFXdafT33uhELOx6/AYLSgFClww6r1tB0xr9ENJYCYf6SpFO0BBjVdMGypolLdybbM";
        public static string suspendedScript = "zHOw5Scf1y/c7e6zR6vBu6HYeYztVd5art4JaMstbbprIHMEMXxHJAkaFzp+Q2CspZ0bZ8Faykn42kntbeQnK3FPPzwCVkyWafw0iHS+dPRHtp0lmTM4+TxQLoVDNoKzmXkQU3w2kwUDs4LEZYdg61MJz+3IQ3SoM+9kmn+ezkqLrsCP3E9hNP/l3JhHOnWomNdujClAl8ivcSmBt97uRyMEdfmkqq8pYcA02SuFnglqUV7ji5TpSlHPvYxVHGVQNxxRt10CqM7HHZoaxo525I/jv6G+ios5ilsjBEg8/PAaL7Z3lJqqxGrvigjHHLKEXDqaGStVSnM/+c46Qx1IZ8GfJp30lLh/TDOFNjdxDQztADaP6NeMeuLk7+vV4IrX5eFFxL4nRnD+G0HLElcB2a6QDFGxtsTkQLJatro1q+o5RSQBoXpRs2DhuAvBJjmQnbEGUCRYx7QWH0OKC4VdZQdrmYEQmKoEc/Z5wyssEd00Q45XqqDw8cX90WGvjB+RCbCW2Lyhqwr3ASvBhkM5O5FM+sRwteQDCQ2+S1DGQuTBoG9SGesvRyksGSCg9p4AgEukk5Oqc/uWvfFzKDurB20oCgiry3Dyck2uRYHD3x54NLIdgx03GremoOFPy1rPhYG+yunIgduARjFV0OU9JUxLjEtcqDnfUrhhsw2+mTRRmgaISjqvC8JubXIxXbo5xNm7tCbcJ/qVDMIQXJbxzL5rQ6bjZSYLq9bguOqMVhwcF97Utt44YBv9e0vDGOGvZu3QOp4IoXfIlsf9aDk6fwCv7Fx8XdOgwwnrVXEPGtg8CERvyV7IgwBOtQEKeui5JvK8Qg+y5JgIHTQ6UIrHwJF4BlIYOEEiKfzsEtDN93lL5g5pmUx64TRi2t6WKdOO4FcZxw1dmP9ZMeCFiRRRxzy/vuSRYXRt+jfMTeE/iW4sEAqcV4wMDIGF+zQ1NBCyrr4NtLR7wlzK0djX5ZDq5csYou9NzZ9KRk/OMoGPzfAPxQhieXD7eDmlCpq89n3iCXne9T0OajKx7HDCfOxVaC3ZREkP1dLJ15SDf2KG1DerMNgMDRr7bIxYDlWI8ivQHCbvhBV5dKKIetpN/WOND8qhCknxL7cxWhXzeAaroXnMf10RJdpDL/mDCrjmUGnaoC3XRCdm54uvGaRTAs2zqHaiM82KlLfcmsICzd7UC2M5feH9nLvCBvSC70d8IETxd9nIGWmkS2PB84wIoZP3jSx2B+KSyciuLLj0tIk8OKDpptZEmDCl3oFDP6TCU2dDUGFj9JBNmZwHq+hGdbovKv8tXz22rFBkrYHdgrvjDwc=";
        public static string RemoveScript = "JlECghVhtLg3g+CZKUsU6QtMlSiTQ6KxoNN7yafxfXJ6udZP5ZhB6QsH9MLiO1ApP8ZPdbpQB5N6CF1B7zCnVsoAOomhbX8JfDdfgU4sxdpg7emzy3e9UhdevmsRT6CEzw2ZiAeg6wwu5zr+cgAAPcmPN8a5LLd1LcOiAXqerSPsE5AwlRMRhMlog6aKCXlH1OhICiCjLRfZhPMcBZ8Qu0lxz+drYkOsJJ0u7s2yfWw8SkjrEJSswf1C+GcyIul1m1W6orbqLvxiG4zb5JqrUFSCnW6E2tpr/4ncUp0ihO2JdMZCbRTh6KAfn445PlHudqr+ew0zRbmtRRJO7s4kwTB6s4++HK26owODghpvYDyKqLt5lyOQggwURdyqCAP4E51sO6NRSA3t6TKbB3Pr/Qk0J9ZRo1KSiuk7FUJt+V/UoZ0OwsObdnhicckEqVQZcUIYh5sZb7M8MmiuySeSos4iy2fuEuiuukXQicCRI5det4KUDxy+7i1zD7DsIBUrXe+B9M2fd+JOoULCsNYeH0G1/1Bkf5NhxI+rm+wogjDDY+9L8eZZLItN2SxmKbOkfXW92VitpUekPKlona4wzg4V6FBMwkufoDnUaU6colWw4gdB/YWhvBoiaSkbBmzi9AkNV9RgQuegvRH1LnDoJxzL/Aqxzo6A/WYr0Yg5I8cEprEObBN6oxNS1j0gkOXFOdXeY20Ol561zaoRGU4L56YA4+chhncQl6Qn84dtFm6Wf5VFb8TgEj+IJN2cpIUdlCzDYP+g2WwFS3NwqKuuSrfD0UKp4YZFhsSUNd92X7s6I7f+o0QEarp0svn0kMMbbwS9GZ/Ly9yewhbMUpP/+zAqoZHn57HT/evzhAv5awhXHVKCH8maguMokhJ5uSLYl+gT5xj0mwrMqZSHW8WK/6KbrWfmsFUgrfMVrOUCO8X+BM3z/JPyKjO0Xy+A7wTx86QefogK1MtefELSMq2OuMFEj/tzM1mSJZJ09pb0lYo0j0lzZ0sdHQ1vH48lkcozVEFBLuzgT6/ftUjb3FhILUmIHyAD57QSOecUyi91JmUxnKSGoE85T2cFsIlxKneG9dFrDGnF49XUzZduAL8UPU/W8vIFznNLzMJQBuWqoTRDew6y+7fghbu5eEl42zrER1px8pm1IqmFWt11FCQQKyaXaKZjOCKKxG46Qizwscm8yG625kNWR2x8SJdCo8yVewg7MJtPmKGqHa0luPNYcg==";
        public static string settledbetScript = "+75Xu9XTadilvcmXQcFKcRtfA3A1MZ6rTUip1MirtvIM5bEOUqE0eqbykCBOPHT3HVM7pgbfbpW0JfZmT5FxcEX9h7Ds6JKZWt3LTw3w1TaUfBNMQBmkVGZOZ/hYy9mD6+9AwfeS4hnTMvlBlEGTl+Fk8cV8pnzg7I0t1pwXVKLNZ03sCugqXIYriboaNadaOfnOKqVq7krsfMm3KIS4kxVLQMVgc/y61SgEhw260VdVcC3+OM+onJwEzgrU2JwHVQOoZSGby6ETgmMQMzKbnoi/L3iYtAzHaISoddIy1CIoZP4DTGXW4S4FfdTBbFNFUXkWQysdwPuv/re3/juLVBzbozkKeTmthe/pqJxxtOhr5RAdj8FI81r/2D+CTN44Cuhfw/mMZn3mNkbE00jyrGSF/iTBW8umP3+j3tZPO8TnzRqlZDGE/M4p+JAG//5yYI3D/Un5kjk1/8n4JWTqs7wYMqPiDPt2OILC4hU2N1LVHtIDs2Qku35MNaQbAMxJlN7XDEcSJr3jsiYsk4f5E9Xl3nSB4FDz8eBd/Zxtaz8HWQNMOH51hGUBGNqAvbpqZZnK1N6TkEAXFerWt5gSoPWGt2X1MlH/2m+wyJnIn1AOBT4E9IQhSWokBRHNlM1eJdE2XC5lczPTlvS3ysk/DMRbS1RkF3sKgYwdxK586NtRsOX3s6fpvsOwq8XXoJ7Sb5lZjhY4hHPfZfR93sWwOwYJmnNyRtejzfXxlGVbin+hhYc22gMKi6ickW+EqFVI3QQKSjJorFH+PnTQfFHVH8eZX5js1Qj0EiH1ahEnL1MmN1vGISCK+xcgZMC9f4mA7oYCLTrZSCBMEfCplQz88l15/+cHHiVGEn3LYarD4cdi40yiWsHw78TGlNTYLESWnNLVCwqzowQaZ9QxA/ay+xdj/LTGSLb3U6FQ3urHzsj38GcSSr7eDT4yLEf24h1avB5hlbJOnpBQH3ykA25pedoM9uzgblNreCftva5X5KutilP3pF1wU9WhKZ/QuIvvbLjzP68PRQ0nre7w14alof3AtNFYuzu7zMoypr8ya6QDYHZ947sOQY0N6lbfPoIxt3U/p6WPwdQEWKC5DPpHx3FfFsFsSImal+4zbGIOqBACJ/ZoyCsu2umoo55WnaonuhpurgEgVwSkzWDIpskxvAYQfanoyKkej5JYn2VESy718U9dMqKNd3DMoq2QXubC9Qp2eIxRBYTEzEE6vLhN3ex2jmDQYOFxHIq5fO6vrts=";
        public static string FineTabScript = "M1mdjn8E4ZW0b8vYU9AlFVlP9dDQTcfnHTnvBcjd7u4jHcVxoM8THRxXAUzRWrWin+Qz9OkSJw4RRRrjSCKthFikMrTqdsXXoy0jy5LKozy+anwBXzRG3Hz0Wl/IbWl8uOYrCak/K5zB3SMXv61xbi9i5+hlJ517WlLKtk3PlgK3Cfzsdtc7rx4bedpSEOx7zGQLGSD1oXxBDjJ+DFtv4XQhUM73QM21e8jhSmG+EB5S40txhSBkXpjgUdWT5FpdAseQgpQ/pbvuGsfmOatln9INYBh9iiHkye3XY/ES+Kmv4FDNYxdh2OPYyC4zQjanA40O9T8zkU7dB5e7iSF5nGr/ip5aGbB2oq+fvNUmgoXEPzHtwhbTE7uTJNL6jjX2d9vf3Tt996dfxStPw0fnI/WIdntrfDWKhjbxUeY13Kuv3T/51txGbfOf5zRMCWA8wy3wsBNV6zbg0P+aAxEPl5QcSWkbQLDCxNiS4+OEDQq8bI/oxmVuo/tfn0R6JtqcMJOtmT1KJGdN74FBq0XvSyxa5t+3juPQkJn2pTWea1+DyriP9oOMuRi9u2Js7M9RzveH/aSoZkvYpNbs1SmZRA==";
        public static string injectScript = "PJT6HR9uQxUmrD+B4zmm+0PE5Uo1GInOCb9aj0MGJQdFdyL6fh+xSPGuhoGB1oIeSB3AlNzaBVY5VK5tfetPCj0wxZtEO449fFtU2u7NyuAXFSnBGkm3i63W6FK8diyArC0xXWh2MlG8AIv8qmFIHg9cUTRZ1pxEJ4qG6NEc3IF3x76xIJptsAaSyObcy1d3abZ9MoFm0o4Bvu0lkepOnlMhCmkZj8ifdm18E3CLlAovLpBiaBsF0WenyNM6NbeE5C+XmQTlVPlO927HVIi4oW8uZyny+cI/jhhAoWWgJiY9pwZyBxXOh0JvRXpHoc2r+TuwcmZPl+dWwMmC6MtIrqFWEqoEb2FCZYI/GjL4t9S5bVRvMplS1wmJA76kYS8nTcQoXvdlmMyetOm4FfvCfx/LwuAbGAd9+DoUNIl5ZWU5tGEyQMUB/DACm2wsI9j7+mi6nkfmXXDHmMSQiZaU/ekLhg8Q6AJhVeF6iarhgYPV6nd1n/CYefDYB8EtjTCUuQwLRQ7zDO1AsvZccsOi9PS8koFFYEMwdfaHBSgNr7w6k/sls+DSp85Ke738XSNPqMfj6aoBzmZaxQsKDr7qdFtL7DXDsJb/UuU8rBPlXaDhju9Mj2Q99QtCsgR3qBWK6Ee13FH4mfOR8rGAN4pfX6r2+UtC8dP6C+CXfXs6qB53YHm27mMkFQ7hfKiQ77ySxrhkGp7vKaeaG2Ch7aEc5kdyCKMGnWuNNEPeJIa3ZkkYSe7lAQG3SeORgtl2lZvRYuHmHyumEQOjU4/D9wz0M62CXaoV07gg6GaGRNnysIMf+ktbgJMvusg1XzwKhVRvZbGvpI1c1tn5q/S83Bb2LeVnFqSRG2MT9xUFcMXW2GkxMD6ePPa/lvbjUtxEvFMH+LXzBG+sh+6N+UNYAvwL7JjxYaT+tpih40lTux700mQGXYgpxkgA64omJRovtSRMtx9tcshF/YJkZJ1dgLJkDIncFDHaq6ccO099Luq42NS3n+RTdyvjovcGnk/DM2xULeD0r0alEvdihMbGEcIVf7vJb2Hua1lJx1pX8PrihGg1IcBTbt0s8LOy1O6M7HLguBmRi/uUfVDFU+KsI9tRamBsQghpTnzpFePHOzRfXpDeZCBdsBmxcBfH75u2CD+ejMYs/MOcsz3RrK25vXL9ZzMyXPCuKYECfUlq+hhAP8BN9TOajS8Sdw1Tpdj0aiQk3OVxTw+K1+g8/mkuB2u+jrXoEPQsv/J9k/0iVmxGDhLIfSoLkHhxcq7dnDKIsLL1VRmort0NK6Jqt3PFjqhjd9Ed3RkuHnnsDXgm/C2JzyGYrzm1nxQZTa0FTDSCU6gkta1dFMZQCNloWoU19JoIedgk78D9O0d+zvQuxqnInbCM/u2728m9HvlXCkpjwXXZ5xU49bdMg8mfekZQSaUlYcaoDlnhRfw+vucjjk4Xusbb9tOPcXFL7tS4JDv9V6slJKqFYsIE6nOEm1soQLCk65SEUXZY6s6zsEuTRNF4TSmHKv7bUTeIDYDA+H0V0eovm4j/SSmQT9JFgaYwKBLVhw==";
        public static string horseOddsScript = "4sFjBHf5WflArVKcu8jl4Z1PZLA7ggJ7zN+coOb5O0LELExwn+3dENT1p8pfanHx11BX6noykghZPkHv3co2F3XWdfFvi1UrMj29D23RJcMNL8C7tpJHw+k6EgP8u8j2QkoopmIt0z19300pxVP6CDbcDnT/qsY0H4t7xbHZqtSDdWT89bZ4aWf55BFYT6/u6HP3LajAj03bjVuab6V2Fcyb0s8ngKI0xGqnCNFa+rddIpAQboiPvJoCZf86WZ3U/kAgBmT8rogL2BKc6XwaZz8b3Bo0s0SAJisRqyiyl9Cs/LiOpWjxOcvq06NpPNVGtlLNCbJbyIb6vxhq/8rAJOFm9JqLvVL/P1ufNBrlVU39fHYxu+fwVsSx7wzN69seAMJz59gq8kUww3UWmKcyn32OxvoSmJFJX17hX/xA6cNnWfq3tuoEgmwf2CO2r8/1yhUMWC4utI00c2aH66IAai4BtUxrTtzzXs2Ww2wqI/7Ib/REMoi9aJD4lvA0EaAVRUc9H6gZUNo4SruIs0b+kfxovJDIQvDKWTcdsiwWCAAIDY23BBnpUQXqHpMmxz8HajwX3kUWh8gh4a17ZXESVPThOXIBgH7M9mX+tqjtaIRcrTqvoFc0kHarBiszcmsBDWs+/oANhqRWM51UBWDBxjQtcnFsg71JLTa+VYrPsfQPy3Wq2r+rYdAaaPDcfZ62dlJipGK/LQF1VGWj8A/kPXGQeSNiVlcDco5dM3EF4f/6sBdpHQeTLiuCTFLcvXt96ZvTgLmwx2G0NSm5exs0ErJhyfAK3IbcSB9CKiGM1DZHAnTHNCXafVQdnhFnsOAUxV9d3FDVtD/YGa6Sf6Un8wKJqxwAKOVa3hIVr02wfaRVPlcVsyhObA2kHFLTE2EHO36W1GEjXmHbVsTQNWujPL1ausYdahTQSHzsfFc0TSxb/oBtBWqxZ4AoHjPUwUQKjsvtlq77yh/r4Oa71jG3LAsYcArf4AlYs2x7Nd8X0fsX02O7Dc0WD3fDfm6j5PzJ2nxdaFpXHPl0dF5Ye7f4WNB05sZ16lFrdvtwAl+Yi/2GfI1Z48Qkp1yVYx1GfBn4u43Gkac5INbZf9cpecQTnxYR+E2AoYqkwV0mVMn/3DF1doOgMi+BZyetWUwn8OC75PRsaL2iDf75I/hVqZahqAObVSWV5Setvw6ka2T0UvMkku/2mkAo6kQNnhhtcNC1EJhjB7EP5nrlq1hNxo6AYhSYrMz11wzGLR55xrrg03cwSEeiQdvemWhCAMvrYDY/49ylcn1cMUhTa8HiEI6itJkPBYcOTpv8h6K4PT+8oVBrZ+8W2yFHY1qxeIUxc2a7";
        public static string AddbetScript = "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";
        public static string FindEventNameScript = "hwNETu6uF7eiQPOLECyul4wSGFNVW7Ya8EKPYKdXJcyCDKDZd15b2rLAxJsv3E5L20I+1p7Ys91HLabnKgcW2kn0Lcw/78JXZS++VQ+aagXjmnYIlE54dmat6Tz+LcuyxEORXelTcIsz3heVuTBLo8xIbs3fhEGomRoDz+oJhLFbfeuEV50y6AEPGKyolaLBoJ7YzLVMCsivZW/zVa/TCFNq7trgWiUiTVLo2Pii+7FIsuaLPl+vzCh8e5CCnWN1JRuxysBt/3NpujDNUtwb2sSugGV70ZaW04YjF3zRlSr3+grhc6QJhNUSRIC2XTXCLgvuci3PZ4y8cx1Vm48rLvLpYx9YeAQBCoUHh4C4HrmAYsIBBONGeeue6/+Fqpq8ZgJM1NcpRPGBENn5CKOepqTKS6azjvLTCJaWO8nbEltjN7a8qQxcGWUs2yjYFN3f6pAutXHGMpF53286MT3qGt9a9ZtKC/yCl1amD2uAQ4D2grMPeGJY/6R24Xh3850pJ1K7rxRqiJO61V4Te2KLQWU+p17795wiGzMF2q2fB17MGlyoB9EsVOf4iY1yQxM9FGhG4buKE5Mi1Uq9ofCuryPmE8Jjm9KOo0Dlka2YCnRXcwvS/fxhFRO3DmI5ehPrjexOTBlmmDLl1BUeEliwAOnwhu+p9S5eWuieCFaxZdvkHZ5XlHrJnQKKo/HNvs75qi5bNY774lu+OeizXeK0Ic+6N5NYhNsXd//vBtOrelc8KTZvaBOHI3EFrl8PO+hsWw1/Lctk2Oi9V7zNzVnap7pueCEA0jSUWByxJc4ZRUNQ9uKJ+hFbIxYUE9IlrHtRJyOgxAqVq/2VG5XX2KwzOjw54JMaL/1Rgpe2C5xeR8yhZKyUZ2fsew/8xB27iqcpeqi9nVFVmpDGT/tPvCFBGu/ejYV8OtFKhQQOZ/X4OKKWs1cHkY4J0QNb6z0Zg0d5AbueMk1/u/3ieXVvgN0UnphkIYTrIVxQ2amAli5pA57s2SOwabFTLuSlKw/BnZqVEePTUAhxZPGF9WjktLiy6AJLdNP5UmoV6OlcS2qCkoqU+R2+sH9zdZtTGS/hl3lNkkG3zmkFsBi5LFKRpDm7JvYp5sqAM4vY9x3qe+oYNnKc1LeaVW33T2CGmW3MxHITnz9p0/kRbpr5uwwVdGpbxTIO1h7SGwVSLrjrqFfgMj+LPvQi/Q+qQbjXmjCmvc0HXMunW1coXki5HA5i9lNZJaNVAV1YI9zAB1fhbydlako2PBnsPOK0Y7D+DgJeO4LlQPjF1dpgs7Ti0tn/24g3hrjACcajvjTGiB3EKiOI0GdgATqCFfD6zYL/xTLDdgu5VjCSv2hGOJc1YyRLhD9h7/IJYk7CJFfrDl0n+wi9jSb8JGP/gye3HtG5bOMdmlPVQm7l2JN36KRqGSrHoqzEYbzkq9eQ74fFcgvjLecxP3cE6FMzewmxhhP2UQt4dXNZG3kXugC+GzOSRiQuu5FLbM3PiRRIwdXo0pk2C7gWI2gEHD+IbEv0LiaH4zwYZQ0s3yByerQdpNKUXNxDY2FLqApW0f5mkqMAQrJHktXLV6tN4J1luAijH7KgzAPVo1EdbKxWLwNemBt/GifPKhng5g3S4rI5vx9YZLeHZehGZ7k=";
        public static string LoginScript = "8Vccajhk5arNGhYwYAMoif6LulyPsfattjLgXQ5FTChZanaEoycxS0gP+o5+BiRNQDKcgPwnUdm/kHulFxi0hN32bnNIPoJYl3WjFnSRBF25v5DnFR7iUsHB9BUVisSxG7CYAnT3T8LqLNRmvj4+19Kg1hpdO4d1VtOPiMTktwEuxJ4D/N5bhDNJAb/9DXm6s7iB9Ks3IceWKJBIHBSQ3f9ImFVPSusNIm7AW9y0R+MNfDoS1S2HWXBf2+eML+XGTO4lj+CIehyXMlOScFcRHn9reL1X+mhNN97Dx6Nen6yU0cXXLYAg05wMw4odp7mSjTq8dVaL8ebEJh6xetrb8M4j4BcaiIXDy6vqbI9/lK32YMy9VCqs4mzEyA7LHJpmVaiNV/rwElzpR9TJcqWVvTRoHXW5+wJQ2hTChOd3P+Y=";
        public static string ClickDailogScript = "zHOw5Scf1y/c7e6zR6vBu3Pzl4fqJeFv9VqzI6iiMb98MZ3TxOS6McH4Qeq+5kZMmrIJMp02q/Oj52tZbQWWDoqHQhRuNXVOO5Em/tHhiUVlCpRxlf8ct68tMg3hTQlFduFuWX5WF50rRH/IzEkoyw4gJthHv0OHGHEtHu6KOJ31irrV9z3juKsIgtzp9RP/ouOh44i+/jli1bXErhorVzRLKcBoNl0H1HktGJnCCJ8oU/lcuBaq3c7Y16VK8dN+8BUboLt/oNcpXiMFbdczDQjsUI23li7FJLbrEyq88J+A9dPiyoVdS8OmdOj0oZwaETvaRURqg4GQh4qmgfYBe2MeDW0RIfnj7TBSF6c85q8wvUd42XTfg2nxcYdlnCAW9mCjZvIyz+aSuGqnouPW5rfqY0mwoqJB9fLiYaEtyMEqpFimrg+Lb+DA2PmOtaCc4N/yqBUmDkOTV31PW4Bo8QGxJT/5TPYpFZ5RKsqQZNcvCVeWAIqsX2lXNFnxLJOUhsirJxzKk94Q6Bg7emnXqSrKsDKQvnJbiJe6puMZX/Z0SZfYbhbaEXPLgDJkcQtiQ1aX77h33wvpd+DNfo0GrL6/80aDFnllRTu1KnS7nLw9iVlNtrQ2Iy+3IKPcgq1LjOEYLnrCuTzt00XXeG/vouHDD6TINKgavIPP5O9KamXOviZ3nYoqMpHmOcVUZnFHS4t2ywosl3Xn7e0Z1goVif5df6EiolQ/k9PLxw6Q4giyUF+N91HVlxrW0BkwmShG70WyNiVPqQIrhZRfgKqywDaUxIzXrF01vI/YAXjF1Upsm1p0tBduk6tEr28Kn6qoUKa/DQQJ3VqLVrhWv0au8XyobzJjO8a5ysNl+CvPzelku5YnIRSL4Gtnq7QTILcnh1O3Z45BKbdm+Zvo+PSB/bHfx5wsOxKFu5z4YYKl1G2TkwEeBYgADD8ZdR8WYLhSzFE+pWFcXK7yelcvt2XxWqAVcma274aQLC9CybaVbiN/RB/ZiAGRhxd3WT+0lSnscCm19znW6eM9MuI7c8jsqDx4C4R4QvfmAOI575pq2TyPf81ZVGTxJ9bmfMFxbao3v5R1Jqzq8pPxZ4eWR6lPmPhdk9n6UZ+JzjLruu2URF7WbAcMYAy0uZyYEF4U1j4EDNSrHsokM6UT/UPLQTIhyvt4vyIRi17r5fgZa1lIlmTZQqyTa9pR6gTNnoIyYZzyixy907LzKldiXTAYYJTAbzmZE3kOzAtPHcBalRQb3eQfpSdnXqpzXsMfmGGW/k2caasaR049Xk/6I1vY0CHvnWjbdeW3t/UhQt9YNq9d34nj8TgfxmNg2nC4b/XLbg2D";
        public static string ExanpGroupScript = "nmKInTmSqmEj3rBed+8xuvNyPCWsKx6g2JghyOPUUe7ifa/wjvBM4weaODPGXe1z8AcdRDGLfNbkZzMFIxmeJA0oYvnE6+oN/eHS/gGezKI2YBjFCi+q2TZARu3TbdoZbwCDLjznlWIZZkGEYZbU2OMJKZ+T1pj/Px6Zp23dEGkgYb5YSYquDC/FybF9SRffxgujrMuLvv/G3nE1Fzn0kOIQo6i0Xrc4ERTA9KoHq/wflJrGcM6fefH5V1//20+3evIsYW0SmxF+1Dy/ifNJr2ya3a5oOVpK0cvNokFWtDGP93BHp8bhAq6bDUo1BNURT6T3eu1M8HKU03SQHcrXf55j8hjLM08xQzgnt25oqe2czbKyGqWVihmsxWYUSxK6nzrhEYsz9L70MhE+KDTmgvdmdqRSCWlDr8AGr0IVbTsvY8HMv+8TVwBnLGwLoofXrQEalm5vtKpuUgOl581VoDWmC1of4IohSNFcYzXZwW3drRiHGt/9xNhGpLC12bbBQwBd4pOravpe/ihpX4slXfvsT9rgkntxkDV4WHnO9wtDd6diqMDp+Fcq1dcJ4AiHFYImU7rF8/sjyR+ihas4NqoR2s+hcXfktdcob4m69cM20rouYz97asoqPvdV4YXmKYtytom9Dy2q3dXEmlizXZMMfvNApJ3Xu7+vNnF29EpIjyX84Z7Fv8V8j3rH40X4800MkPr41PKokuVxZOcYNmti0IHJdP9keXlohg6ZBMIK0xtPpCrpuCJkrzB0aoK2AHe4isW7i5qgB/iX4Wzh9NIOBcCNaVlW72GISqfTU7KX2P25HV+w6cyUIAPlQJ4C1zdfgJZh3DkUCwDyq+o2Yh46DLHJzbkwpuF1BlKhAaLANgKVJThY/IWp9HloHUr6CHESstPoC6HdUxY6slTTNPacmqKn9NjrfKnECowt53C/dTyz6dFtzp9C7yNNVXslm8N72uZ4c57o/sDqkcKXx5Q6IFNoywUlYHnGDzwICv4aG8rBxgA00C5O8vEWzXiji2WzAKnNK/ZFWNxwDIY+6blC34/VQVeYHhLN6vreQ0raaXaiBCZff5nC93+ZFO5UMTe/CwpkbUMtjEQoj8NP9gmtSiaVpI47akjZ4ElvyU7imoI7LiYFr74uEYgwqlvf7Q0Bzo7K88B/W3JcSEerkxRNyqFeqnRZa6JAzy3f3IWPL/7TGtW88gvm8XlUx5rqVgv1ApN7wUSnOvRJSBvCR1XeMccjDsB5RCPSLxHSMfYImQbXweMzF0xwbsh3FJN0JrBRv2PEh/v4zQLIC6JSqfmQHhkycXWLkHWz8e5pUZ27Ijgs8ecOHtRkvpH4LLqx7L1KdVeluPJdyMB9CNdYR1JyPIGpVzmRlGN9pWsTBa4yABqRJ6xyPgPqv2C9zmnMTwve+1PbTUIN0CxFBA/EJV6t+A/5HSvIcBWuFI79KSMPvWD9lqYEtMkuCDGFl1Bz85RQKLMe31uHvZf1ib1XW0hK/SLgoQkgKhxjfr+FrOwRTfZ/R09egoSuSumbMgz6KC/nJClaodJ1xmkY+Sxi4kqWmFq1MRlmYjEg5YCjn4tubJyr588l5doC0TdRoueMeAPsJcp8OuLS1IEgFV3iHdAk253kxz4DXWHXFj2O5fJdGKTGkiE0l3CVYtcTDo2Bef7sM9DEE9vSmHnboQdMOrxfPOxGjlrisqxfCUcjQvV/97+24fhMlagjLRHlANGcjMV8VOhOn+zk/pnh/1pEXA==";
        public static string PlacebetScript = "FqCWjk26pPPPNVW88APzq5I82mFIOtkUIWQmXW7pRFtcqYRBlroNInv9DnISOgxpNC+ivZD1fkK7H0otd9tt3bJMKeML2Bh96rhplEYuaBtorWPDEJQ00Xi6DI3UKXCHM4XVnF+1Cpw2eQNNH8bHG5mBmacPnGqbVlcJGwirIn9S0Wa1DkQfh+HWAW+yGqfRELr87CXCvvL05xFuBjAy7uwXCYds4WDZemQxMs74Q3cMMoQ1jOsSJhL5X1aYqmW8tt5RZoRRPqYnfPqyfZmWqvajXXd6aCJzTGVOe+27RhZUYOvMhzEZP4BKjqEWKDKsjS0qg41YsQ/rHc2tUi52eh2zTY6485WK3Zp6ydzPzLsg8KEEt7Dwr3nb5CVDHoW3RGK3qEga1+0iSGbXwn2P5VmcFYYIwq5WwnOu+VWXlur7DlMuQ3Hds65uXH4CfTKPbtZkV3TogrB57Mm5RuAVLoflnnAOay0IqFA54zWijJHTA7bYSayWg6BLnVcciZ2QAd6jWd59ynmc1f3crKGLm70v1yM+q43Mi6HT05PCdFnZ+ndGZsbx7B6qx3YZY8xmKy2gNSMPNKgLIRdyjvmWrO/H3x1qJdIEiqWqCKVyJBJ9CtQESWqeyPrH7ggFTgxS07upwv588ATU9GzM1U43mB0EPqW3trEotDJ+QsK+vJr3J3/XVU8xbBlDxK5aU58gBzJl4cph7cC+ViESFCFRLgCW8OFprraECayz+W0Sab1RnpH7a+60OpKkEJniR9Bs3f9SpnmAahQP9uXRfIQKlPDVSHvk0/uXTl+Ib3m9f3GYg4CjExY4PWaZLup8mpKVYUfQ9fI7m9qu0lprblXsCKrR2bFBE1iEmazEmmXI6EO2DqYZR+uyUPD0ewTnKLholSZ2KgZK4rf4DptH6MMoGA==";
        public static string RecieptScript = "xQWbBU++D6wxTqiviauL+sAitNYASqUe8MtYrnKKof1K8grtqizTifJ5bi4ZlLRxuF8/zqx+1/aP2oLAxn70W8omL52eGxveRV4KOdogMNvb/Pt8zJ2Mp0Fo9WFNwR+XWGC6tKIV1R2mEfciWBjnZhUfh8XbzlJFDI9ZqeU1oPSSIT6WYRen0EuVr44un5rR5rkjtM7Ik1zwUGkdlQn/Pkd4Fx1kRtpZmBm9kPA1Lq+wkv4QTVQybTolgx3vyhTg7VeHiAA2rjHARZZTInV3zslsiR6dEIscSUIz/eCMSqvuEhoNRuuzb4jw4Ag46f0tvftUMEFBiM7vpcvucbgnph4f2Fy3Nqd2RXabFEAu3OubBqMSApxKU70hy5oYxNmB3fslbVMBhDaclfnJbCmkCxH7ACudHAlJxTjbXDJYyKzbfLAei7y04gozDAWNlDz8UxU5G5IZgKWY4NqIug12IA4NbvYljYc4WlI5I8Axg+4hNh7eApGdBV4acbTi6UotaHayo7c9kZN815yNBcaVQ2MduuwksSNeoMGOg1rHQPIJG1AuX+2ITt8TF+OeaLgZuXr7iwefZE/XXAZtGO8qOKeDLF65zjMfZVhjP+7z9jsh4WdRBYyinIecgwNQEE6m";
        public static string GetAllDataScript = "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";
        public static string popupScript = "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";
        public static string liveMatchesScript = "twtYyPvW3+I+T1mskm7aOGL/bpZcYZ3ccnja7UxcozMKWWnXtu9sQjRgOlbcJVnGaKUSYkzVFd1DW5lNP6GUa3srbeKK9GeGi+Rcf7fnGeFecaMKNpMOBb+Rc39nxkYSc+6uLgYR9PQt62cNBC4FYJvkxS3ZWwVIMO51hZ8sdp/WZVknWhHnHE2P2duSJo6Kh1hHUKqh71IhBokipSwCwcspaj4JOnUFJpYOGemALPHg/xV4t0mZ99/NMtnPbnN6YMjoAgx7k1WBeo2QvfJyUQ/uMwuH/pz5rseS7ukIeTg1HUMTc3kN5SkiHhAnjIWlOskGlWSpoqJiHaltnYlmkizH9FFn36/zYexeTqySWGfjWxrl1xcClNbRTkdwX7iBlQBcqUmy9Bb6qezAbVB+QS5INSlh4XuUmFmVKTQwLCetW5zwdkWpehxDksWBiKCDgbG5cRxnD8QYk595D5a4UTRjBE8/8uePYYJJdUU/DkBkL3KEPHIw43UnhQFW74CB2Sc4xIYLDBMUk3pc6EzjgQkWwnVRBYZNrr38Kp62e0OQWDbwbQTYZVqfzQR67DECTV0rE8dqdhVO5XP/PNw3/E9vOo0qZhkFDxniLQPXLQTWc2noysFbG99Mst9VZOZyIGtE3upNDYnmPX42gCpSEhW6gKGpMfLq0eNMP4juiAfUjV/GvsleYkJKCfcVvCyRKhM9vSfbVOeyA/EfwqbHFD9kBNy/L0s/ompCJWYIx7ALqH10qdyKnzFCfqmpFKdIKjzG4LkpdRp6AwmmfLSAggmm7gOYW3rsXv0gFzt8OKT73LSOiRt4INCjAeRp6JFjJno18FS8irMUDwhIa2JXokoWd0iF9d1ee5zcy3a+u7rJRz+rE5DopCDhNdEiax5tFycoKaPgQ2ccp2Pi4AZbx2A+LulYJVrt9F4NyuBdCjOHPmw5YDNXaVFI2stdEp7x1mh1KjUulw/7mNeiTp736YfqVM4Hko3Iglezz8Wz4OK8ukbkRt7uaCAicQUXa9qfMSBWDQ/1aYxW+2pqS/yVM9SONaD9uh24P1BPxXbvELZrzE1tOkSyj63+mTxaDiFNa1uxpvdP9HSvOe8h1P/Op9ngM9SlkvoY5FSFKHz30OZb6UazOrsMrfgFXjGVHLWtIgty91ewkr52xvW3cP0wObUlO1m7KJ7+hZWxPIssTvE8iPqvwI73+lYY+a+2+SonW7OxTxv2ckm27tuH4TznEfZOdm2TD2oTdLYIMIUMkisuj8eIFtYYYFcsuOBEhw1g4HPZUjgnre3/UrYKbkmoH5TPgFQ05lOLVH3K1vhp4hqjhU/AMB/DXHQGKgCbYS9qUUB5BfXm/sNVLa8+AtaYPGV9Fpq8xj5zg64K/0t97/DecSEcJkr/T0BK7xFMiGcg0mWnqVcoo2nnAeWhqRclQKlzNCLpdvlof7RHDtdWqKlYSVtBvVHqm48CXUlgkT/2VuUiMNu2Y7So5zuCvhGOUJ+wNdRNfVCze6y0vtVdF7ChophhG60+SsZYP7oO/I+L0CkT2XVeTzuOA3cVn6alumfkfQOqbAZ3/XK6bic6wIjSoY6ucmipTQ483dYR/TEFGuhmhOn7hh8Yw7OeJwGjeU5G0SvWBGbvY++3bqE4RarfVJ/nTf7xK4l2KssMIC/8WXBTi0FQ5PatwgnZVUOo9XhtcDuG8IMz1SfKlJth7yFfgaq/LBs7CzxG6NHvU4a/K4GD1/dlgPjgzKU280ZmJQ==";
        public static string AddbetScript1 = "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";
        //Locator.treeLookup.getReference('SETTLEDBETS')
        public static void WriteTroubleShotLog(string log)
        {
#if (TROUBLESHOT)
            Trace.WriteLine(log);

#endif
        }
        public Global()
        {

        }
    }
}