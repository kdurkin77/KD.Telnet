﻿[<AutoOpen>]
module KD.Telnet.TcpTelnetClient.TcpClientExtensions

open System
open System.Net
open System.Net.NetworkInformation

type System.Net.Sockets.TcpClient with

//There does not seem to be any sort of indication to say all the data has been transmitted to, no special characters, 
//no special message, not even a length field. The assumption is that all data comes back at once so this.Available has ALL the data.
//This is suggested in RFC854:
//...the following default conditions pertain to the transmission of data over the TELNET connection:
//   1)  Insofar as the availability of local buffer space permits,
//   data should be accumulated in the host where it is generated
//   until a complete line of data is ready for transmission, or
//   until some locally-defined explicit signal to transmit occurs.
//   This signal could be generated either by a process or by a
//   human user.

    member this.GetEchoAsync(lengthExpected, timeout) = 
        let startTime = DateTime.Now
        let stream = this.GetStream()

        let rec ReceiveDataAsync data = async {
                let dataLength = Array.length data

                if dataLength = lengthExpected || (DateTime.Now - startTime) >= timeout then
                    raise (OperationCanceledException())
                    return Unchecked.defaultof<_>
                elif dataLength > lengthExpected then
                    return failwith "datalength > lengthExpected"
                else
                    match this.Available with
                    | 0 ->
                        do! Async.Sleep 100
                        return! ReceiveDataAsync data
                    | n ->
                        let buffer = Array.zeroCreate (Math.Min(lengthExpected - dataLength, n))
                        let! count = stream.AsyncRead(buffer)
                        return Array.concat [ data ; buffer |> Array.truncate count ]
                }

        ReceiveDataAsync Array.empty

    member this.GetResponseWithTimeoutAsync(timeout) =
        let startTime = DateTime.Now
        let stream = this.GetStream()

        let rec ReceiveDataAsync data = async {
                if (DateTime.Now - startTime) >= timeout then
                    raise (OperationCanceledException())
                    return Unchecked.defaultof<_>
                else
                    match this.Available with
                    | 0 ->
                        do! Async.Sleep 100
                        return! ReceiveDataAsync data
                    | n ->
                        let buffer = Array.zeroCreate n
                        let! count = stream.AsyncRead(buffer)
                        return Array.concat [ data ; buffer |> Array.truncate count ]
                }

        ReceiveDataAsync Array.empty

    member this.IsConnected() =
        //in .Net Core 3.1 the IPs will use the v6 schema if v6 is enabled
        //so in order to properly compare the IPs we're ensuring everything
        //is converted to IPv6

        IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpConnections()
        |> Seq.choose (fun i ->
            let clientLocal = 
                let ipEndPoint = (this.Client.LocalEndPoint :?> IPEndPoint)
                IPEndPoint(ipEndPoint.Address.MapToIPv6(), ipEndPoint.Port)
            let clientRemote = 
                let ipEndPoint = (this.Client.RemoteEndPoint :?> IPEndPoint)
                IPEndPoint(ipEndPoint.Address.MapToIPv6(), ipEndPoint.Port)
            let local = IPEndPoint(i.LocalEndPoint.Address.MapToIPv6(), i.LocalEndPoint.Port); 
            let remote = IPEndPoint(i.RemoteEndPoint.Address.MapToIPv6(), i.RemoteEndPoint.Port);
            match local.Equals(clientLocal) && remote.Equals(clientRemote) with
            | true      -> Some i
            | false     -> None
        )
        |> Seq.tryExactlyOne
        |> function
        | Some x    -> x.State = TcpState.Established
        | None      -> false