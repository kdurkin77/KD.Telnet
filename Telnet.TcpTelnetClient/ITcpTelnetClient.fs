namespace KD.Telnet.TcpTelnetClient

open System
open System.Threading.Tasks
open System.Net

type ITcpTelnetClient =
    inherit IDisposable
    abstract ConnectAsync : IPAddress ->  int -> Task
    abstract ReceiveData : TimeSpan -> Task<string>
    abstract ReceiveEcho : int -> TimeSpan -> Task<string>
    abstract SendData : string -> Task
    abstract SendData: byte[] -> Task
    abstract SendDataReceiveEcho : string * TimeSpan -> Task<string>
    abstract SendDataReceiveEcho : byte[] * TimeSpan -> Task<string>
