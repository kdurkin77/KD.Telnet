[<AutoOpen>]
module KD.Telnet.TcpTelnetClient.TcpClientExtensions

open System

type System.Net.Sockets.TcpClient with
    member this.GetEchoAsync(lengthExpected, timeout) = 
        let startTime = DateTime.Now
        let stream = this.GetStream()

        let rec ReceiveDataAsync data = async {
                let dataLength = Array.length data

                if dataLength = lengthExpected || (DateTime.Now - startTime) >= timeout then
                    return data
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
                        return!
                            Array.concat [ data ; buffer |> Array.truncate count ]
                            |> ReceiveDataAsync
                }

        ReceiveDataAsync Array.empty

    member this.GetResponseWithTimeoutAsync(timeout) =
        let startTime = DateTime.Now
        let stream = this.GetStream()

        let rec ReceiveDataAsync data = async {
                if (DateTime.Now - startTime) >= timeout then
                    return data
                else
                    match this.Available with
                    | 0 ->
                        do! Async.Sleep 100
                        return! ReceiveDataAsync data
                    | n ->
                        let buffer = Array.zeroCreate n
                        let! count = stream.AsyncRead(buffer)
                        return!
                            Array.concat [ data ; buffer |> Array.truncate count ]
                            |> ReceiveDataAsync
                }

        ReceiveDataAsync Array.empty
