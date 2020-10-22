namespace KD.Telnet.TcpTelnetClient

open FSharp.Control.Tasks.V2.ContextInsensitive
open System.Net
open System.Net.Sockets
open System.Text
open System.Threading.Tasks

type TcpTelnetClient() = 
    let tcpClient = new TcpClient()

    let SendDataString (data: string) =
        task {
            let dataBytes = Encoding.ASCII.GetBytes(data)
            let stream = tcpClient.GetStream()
            do! stream.AsyncWrite(dataBytes)
        } :> Task

    let SendDataBytes (data: byte[]) =
        task {
            let stream = tcpClient.GetStream()
            do! stream.AsyncWrite(data)
            } :> Task

    let ReceiveEcho lengthExpected timeout =
        task {
            let! bytes = tcpClient.GetEchoAsync(lengthExpected, timeout)
            return Encoding.ASCII.GetString(bytes)
        }

    interface ITcpTelnetClient with
        member _.ConnectAsync (ip: IPAddress) (port: int) = tcpClient.ConnectAsync(ip, port)

        member _.IsConnected() = tcpClient.IsConnected()

        member _.ReceiveData(timeout) = 
            task {
                let! receivedBytes = tcpClient.GetResponseWithTimeoutAsync(timeout)
                return Encoding.ASCII.GetString(receivedBytes)
            }

        member _.ReceiveEcho lengthExpected timeout = ReceiveEcho lengthExpected timeout

        member _.SendData (data: string) = SendDataString(data)

        member _.SendData (data: byte[]) = SendDataBytes(data)

        member _.SendDataReceiveEcho(data: string, timeout) = 
            task {
                do! SendDataString data
                return! ReceiveEcho data.Length timeout
            }

        member _.SendDataReceiveEcho(data: byte[], timeout) = 
            task {
                do! SendDataBytes data
                return! ReceiveEcho data.Length timeout
            }

        member __.Dispose() =
            tcpClient.Dispose()