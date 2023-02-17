
Imports System
Imports System.Runtime.InteropServices
Imports System.Net
Imports System.Net.Sockets
Imports System.Threading
Imports System.Text


Public Class Form1

    Declare Function joyGetPosEx Lib "winmm.dll" (ByVal uJoyID As Integer, ByRef pji As JOYINFOEX) As Integer

    <StructLayout(LayoutKind.Sequential)>
    Public Structure JOYINFOEX
        Public dwSize As Integer
        Public dwFlags As Integer
        Public dwXpos As Integer
        Public dwYpos As Integer
        Public dwZpos As Integer
        Public dwRpos As Integer
        Public dwUpos As Integer
        Public dwVpos As Integer
        Public dwButtons As Integer
        Public dwButtonNumber As Integer
        Public dwPOV As Integer
        Public dwReserved1 As Integer
        Public dwReserved2 As Integer

    End Structure

    Private receivingThread As Thread                            'Create a separate thread to listen for incoming data, helps to prevent the form from freezing up

    Dim myjoyEX As JOYINFOEX
    Dim udpClient1 As System.Net.Sockets.UdpClient
    Dim udpClient2 As System.Net.Sockets.UdpClient
    Dim udpListen As System.Net.Sockets.UdpClient
    Dim iepRemoteEndPoint As IPEndPoint
    Dim sendBytes As [Byte]()
    Dim receiveBytes As [Byte]()
    Dim xspd As Integer
    Dim yspd As Integer
    Dim zspd As Integer
    Dim xymove As Byte
    Dim zmove As Byte
    Dim ack As Byte
    Dim deadband As Integer
    Dim camnum As Integer
    Dim prevbtnstate1 As Integer = 0
    Dim prevbtnstate2 As Integer = 0
    Dim startuptimer As Integer = 0
    Dim connectok As Boolean = False

    Private Sub Receiver()
        'Dim endPoint As IPEndPoint = New IPEndPoint(IPAddress.Any, port) 'Listen for incoming data from any IP address on the specified port (I personally select 9653)
        While (True)                                                     'Setup an infinite loop
            Dim data() As Byte                                           'Buffer for storing incoming bytes
            data = udpListen.Receive(iepRemoteEndPoint)                     'Receive incoming bytes 
            Dim message As String = Encoding.ASCII.GetString(data)       'Convert bytes back to string
            'If Closing = True Then                                       'Exit sub if form is closing
            'Exit Sub
            'End If
        End While
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        deadband = 2000
        camnum = 1
        PictureBox1.BackColor = Color.Red
        PictureBox2.BackColor = Color.DimGray

        myjoyEX.dwSize = 64
        myjoyEX.dwFlags = &HFF ' All information
        Timer1.Interval = 50  'Update at 20 hz
        Timer1.Start()
        udpClient1 = New System.Net.Sockets.UdpClient()
        udpClient1.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, True)
        udpClient1.Connect(TextBoxCam1.Text, 1259)
        udpClient2 = New System.Net.Sockets.UdpClient()
        udpClient2.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, True)
        udpClient2.Connect(TextBoxCam2.Text, 1259)
        udpListen = New System.Net.Sockets.UdpClient()
        udpListen.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, True)
        udpListen.Client.Bind(New IPEndPoint(IPAddress.Any, 1260))
        iepRemoteEndPoint = New IPEndPoint(IPAddress.Any, 1260)
        'udpClient.Connect("192.168.92.162", 52381)

        connectok = True

        ReDim sendBytes(20)
        ReDim receiveBytes(20)
        Dim start As ThreadStart = New ThreadStart(AddressOf Receiver)
        receivingThread = New Thread(start)
        receivingThread.IsBackground = True
        receivingThread.Start()

    End Sub

    Sub ViscaSend(cmd As Byte(), len As Integer)
        'Dim sendArr As Byte()
        'ack = 0
        'ReDim sendArr(8 + cmd.Length)
        'sendArr(0) = &H1
        'sendArr(1) = &H0
        'sendArr(2) = &H0
        'sendArr(3) = &H9
        'sendArr(4) = &H0
        'sendArr(5) = &H0
        'sendArr(6) = &H0
        'sendArr(7) = &H0
        'receiveBytes(0) = 0 : receiveBytes(1) = 0 : receiveBytes(2) = 0
        'For i = 0 To cmd.Length - 1 : sendArr(8 + i) = cmd(i) : Next
        'udpClient.Send(sendArr, sendArr.Length)
        If camnum = 1 Then
            udpClient1.Send(cmd, len)
        Else
            udpClient2.Send(cmd, len)
        End If

        'Do While ack = 0

        'Loop
        'Loop

    End Sub

    Function SpeedCurve(spd As Integer)
        If spd < 24 Then Return 1
        If spd < 28 Then Return 2
        If spd < 32 Then Return 3
        If spd < 34 Then Return 4
        If spd < 36 Then Return 5
        If spd < 38 Then Return 6
        If spd < 40 Then Return 7
        If spd < 42 Then Return 8
        If spd < 44 Then Return 9
        If spd < 46 Then Return 10
        If spd < 48 Then Return 11
        If spd < 50 Then Return 12
        If spd < 52 Then Return 13
        If spd < 54 Then Return 14
        If spd < 56 Then Return 15
        If spd < 58 Then Return 16
        If spd < 60 Then Return 17
        If spd < 62 Then Return 18
        If spd < 62 Then Return 19
        If spd < 62 Then Return 20
        If spd < 62 Then Return 21
        If spd < 62 Then Return 22
        Return 23
    End Function

    Function hexstr(val As Byte(), vlen As Integer)
        Dim opstr = ""
        For i = 0 To vlen
            If val(i) < 16 Then opstr = opstr + "0"
            opstr = opstr + Hex(val(i))
            opstr = opstr + " "
        Next
        hexstr = opstr
    End Function

    Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer1.Tick

        ' Get the joystick information
        Call joyGetPosEx(0, myjoyEX)

        Dim op = ""

        With myjoyEX
            op = "X:" & .dwXpos.ToString          'Up to six axis supported
            op = op & vbCrLf
            op = op & "Y:" & .dwYpos.ToString
            op = op & vbCrLf
            op = op & "Z:" & .dwZpos.ToString
            op = op & vbCrLf
            op = op & "R:" & .dwRpos.ToString
            op = op & vbCrLf
            op = op & "U:" & .dwUpos.ToString
            op = op & vbCrLf
            op = op & "V:" & .dwVpos.ToString
            op = op & vbCrLf
            op = op & "btn:" & .dwButtons.ToString("X")  'Print in Hex, so can see the individual bits associated with the buttons
            op = op & vbCrLf
            op = op & "num:" & .dwButtonNumber.ToString  'number of buttons pressed at the same time
            op = op & vbCrLf
            op = op & "pov:" & (.dwPOV / 100).ToString     'POV hat (in 1/100ths of degrees, so divided by 100 to give degrees)

            If (.dwButtons <> prevbtnstate1) Then 'button state has changed (top 8 buttons)

                If (.dwButtons > prevbtnstate1) Then 'new button pressed (otherwise released)
                    If (.dwButtons And 1) Then
                        camnum = 1
                        PictureBox1.BackColor = Color.Red
                        PictureBox2.BackColor = Color.DimGray
                    End If
                    If (.dwButtons And 2) Then
                        camnum = 2
                        PictureBox1.BackColor = Color.DimGray
                        PictureBox2.BackColor = Color.Red
                    End If
                    If (.dwButtons And 4) Then 'focus lock cam 1
                        If (CheckBox1.Checked = True) Then CheckBox1.Checked = False : Else CheckBox1.Checked = True
                    End If
                    If (.dwButtons And 8) Then 'focus lock cam 2
                        If (CheckBox2.Checked = True) Then CheckBox2.Checked = False : Else CheckBox2.Checked = True
                    End If
                    If (.dwButtons And 16) Then 'ev cam 1
                        If (NumericUpDown1.Value < 7) Then NumericUpDown1.Value = NumericUpDown1.Value + 1
                    End If
                    If (.dwButtons And 32) Then 'ev cam 1
                        If (NumericUpDown1.Value > -7) Then NumericUpDown1.Value = NumericUpDown1.Value - 1
                    End If
                    If (.dwButtons And 64) Then 'ev cam 2
                        If (NumericUpDown2.Value < 7) Then NumericUpDown2.Value = NumericUpDown2.Value + 1
                    End If
                    If (.dwButtons And 128) Then 'ev cam 2
                        If (NumericUpDown2.Value > -7) Then NumericUpDown2.Value = NumericUpDown2.Value - 1
                    End If
                End If

                prevbtnstate1 = .dwButtons
            End If

            If (.dwRpos <> prevbtnstate2) Then 'button state has changed (bottom 8 buttons)

                If (.dwRpos <> prevbtnstate2) Then  'new button pressed (otherwise released)
                    If (.dwRpos And &H100) Then SendPreset(1, 1) 'preset 1 cam 1
                    If (.dwRpos And &H200) Then SendPreset(1, 2)
                    If (.dwRpos And &H400) Then SendPreset(1, 3)
                    If (.dwRpos And &H800) Then SendPreset(1, 4)
                    If (.dwRpos And &H1000) Then SendPreset(2, 1) 'preset 1 cam 2
                    If (.dwRpos And &H2000) Then SendPreset(2, 2)
                    If (.dwRpos And &H4000) Then SendPreset(2, 3)
                    If (.dwRpos And &H8000) Then SendPreset(2, 4)

                End If

                prevbtnstate2 = .dwRpos
            End If


            Dim dir = 0

            sendBytes(0) = &H81
            sendBytes(1) = &H1
            sendBytes(2) = &H6
            sendBytes(3) = &H1
            sendBytes(4) = &H1
            sendBytes(5) = &H1
            sendBytes(6) = &H3
            sendBytes(7) = &H3
            sendBytes(8) = &HFF

            zspd = SpeedCurve(Math.Abs((.dwZpos - 32768) * 64 / 32768)) * 7 / 24
            xspd = SpeedCurve(Math.Abs((.dwXpos - 32768) * 64 / 32768))
            yspd = SpeedCurve(Math.Abs((.dwYpos - 32768) * 64 / 32768)) * &H14 / 24

            op = op & vbCrLf & "xspd:" & xspd.ToString
            op = op & vbCrLf & "yspd:" & yspd.ToString

            If (.dwXpos < 32767 - deadband) Or (.dwXpos > 32767 + deadband) Then
                sendBytes(4) = xspd
                If (.dwXpos > 32768) Then
                    dir = dir Or 1
                    sendBytes(6) = &H1
                Else
                    dir = dir Or 2
                    sendBytes(6) = &H2
                End If
            End If

            If (.dwYpos < 32767 - deadband) Or (.dwYpos > 32767 + deadband) Then

                sendBytes(5) = yspd
                If (.dwYpos < 32768) Then
                    dir = dir Or 4
                    sendBytes(7) = &H1
                Else
                    dir = dir Or 8
                    sendBytes(7) = &H2
                End If
            End If

            op = op & vbCrLf & "dir:" & dir

            If (dir = 0) Then
                If xymove <> 0 Then
                    xymove = 0
                    sendBytes(4) = &H0
                    sendBytes(5) = &H0
                    ViscaSend(sendBytes, 9)
                    op = op & vbCrLf & hexstr(sendBytes, 9)
                End If
            Else
                xymove = 1
                ViscaSend(sendBytes, 9)
                op = op & vbCrLf & hexstr(sendBytes, 9)
            End If

            dir = 0
            sendBytes(2) = &H4
            sendBytes(3) = &H7
            sendBytes(5) = &HFF

            If (.dwZpos < 32767 - deadband) Or (.dwZpos > 32767 + deadband) Then

                If (.dwZpos > 32768) Then
                    dir = 1
                    sendBytes(4) = &H20 Or zspd
                Else
                    dir = 2
                    sendBytes(4) = &H30 Or zspd
                End If
            End If

            If (dir = 0) Then
                If zmove <> 0 Then
                    zmove = 0
                    sendBytes(4) = &H0
                    ViscaSend(sendBytes, 6)
                End If
            Else
                zmove = 1
                ViscaSend(sendBytes, 6)
            End If


        End With
        TextBox1.Text = op

        If connectok Then 'send some initial values so cams in known state
            If (startuptimer < 100) Then startuptimer = startuptimer + 1

            If (startuptimer = 10) Then
                sendBytes(0) = &H81
                sendBytes(1) = &H1
                sendBytes(2) = &H6
                sendBytes(3) = &H1
                sendBytes(4) = &H18
                sendBytes(5) = &HFF
                Dim oc = camnum
                camnum = 1 : ViscaSend(sendBytes, 6) 'set max preset recall speed
                camnum = 2 : ViscaSend(sendBytes, 6)
                camnum = oc
            End If

            If (startuptimer = 20) Then
                SendFocusLock(1, False)
                SendFocusLock(2, False)
            End If

            If (startuptimer = 30) Then
                SendEV(1, 0)
                SendEV(2, 0)
            End If

        End If

    End Sub
    Sub SendEV(cam As Integer, ev As Integer)
        If (ev < -7) Or (ev > 7) Then Return
        Dim oc = camnum
        sendBytes(0) = &H81
        sendBytes(1) = &H1
        sendBytes(2) = &H4
        sendBytes(3) = &H4E
        sendBytes(4) = &H0
        sendBytes(5) = &H0
        sendBytes(6) = ((ev + 7) And &HF0) / 16
        sendBytes(7) = (ev + 7) And &HF
        sendBytes(8) = &HFF
        camnum = cam
        ViscaSend(sendBytes, 9)
        camnum = oc
    End Sub
    Private Sub NumericUpDown1_ValueChanged(sender As Object, e As EventArgs) Handles NumericUpDown1.ValueChanged, NumericUpDown2.ValueChanged
        Dim v = CType(sender, NumericUpDown).Value
        If CType(sender, NumericUpDown).Name = "NumericUpDown1" Then SendEV(1, v)
        If CType(sender, NumericUpDown).Name = "NumericUpDown2" Then SendEV(2, v)
    End Sub

    Sub SendFocusLock(cam As Integer, state As Integer)
        sendBytes(0) = &H81
        sendBytes(1) = &H1
        sendBytes(2) = &H4
        sendBytes(3) = &H38
        If (state = True) Then sendBytes(4) = &H3 : Else sendBytes(4) = &H2
        sendBytes(5) = &HFF
        Dim oc = camnum
        camnum = cam
        ViscaSend(sendBytes, 6)
        camnum = oc
    End Sub

    Private Sub CheckBox1_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox1.CheckedChanged, CheckBox2.CheckedChanged
        Dim v = CType(sender, CheckBox).Checked
        If CType(sender, CheckBox).Name = "CheckBox1" Then SendFocusLock(1, v)
        If CType(sender, CheckBox).Name = "CheckBox2" Then SendFocusLock(2, v)
    End Sub

    Sub SendPreset(cam As Integer, prnum As Integer)
        Dim oc = camnum

        sendBytes(0) = &H81
        sendBytes(1) = &H1
        sendBytes(2) = &H4
        sendBytes(3) = &H3F
        If (RadioButton1.Checked = True) Then sendBytes(4) = &H2 : Else sendBytes(4) = &H1 '2=recall 1=store
        sendBytes(5) = prnum
        sendBytes(6) = &HFF
        camnum = cam
        ViscaSend(sendBytes, 7)
        camnum = oc
        RadioButton1.Checked = True 'automatically cancel preset save mode
    End Sub


    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click, Button2.Click, Button3.Click, Button4.Click, Button8.Click, Button7.Click, Button6.Click, Button5.Click
        Dim n = CType(sender, Button).Name
        Dim v = CInt(Strings.Right(n, 1))
        Dim pn = v
        If pn > 4 Then
            SendPreset(2, pn - 4)
        Else
            SendPreset(1, pn)
        End If
    End Sub
End Class
