Imports System.IO
Imports System.Windows.Forms
Public Class NumberedTeamNames
    Public teamNumber As Integer
    Public teamName As String
    Public Sub New(ByVal theTeamName As String, ByVal theTeamNumber As Integer)
        teamName = theTeamName
        teamNumber = theTeamNumber
    End Sub
    Public Overrides Function ToString() As String
        Return teamName
    End Function
End Class
Public Class Form1

    Dim teamsDictionary As Dictionary(Of Integer, String)

    Private Sub ComboBox1_selectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox1.SelectedIndexChanged

        DataGridView1.Rows.Clear()

        Dim desiredTeam As NumberedTeamNames
        desiredTeam = ComboBox1.SelectedItem
        If (desiredTeam.teamNumber = 0) Then
            Return
        End If

        Dim tempDir As DirectoryInfo
        tempDir = Directory.CreateDirectory(Path.GetTempPath() & My.Application.Info.AssemblyName)

        Dim files As IEnumerable
        files = tempDir.EnumerateFiles()
        Dim enumerator As IEnumerator
        enumerator = files.GetEnumerator()

        Dim selectedTeamNotMet = True

        Do While selectedTeamNotMet
            If Not enumerator.MoveNext() Then
                Exit Do
            End If
            Dim sched As FileInfo
            sched = enumerator.Current
            Dim stopIndex = sched.Name.IndexOf("_")
            Dim team = Integer.Parse(sched.Name.Substring(0, stopIndex))

            If (desiredTeam.teamNumber <> 1 And desiredTeam.teamNumber <> team) Then
                Continue Do
            End If
            Dim dimension As Integer
            Dim dt As New DataTable("events_" & team)
            Dim firstRow As Boolean = True
            Using streamRead As New StreamReader(sched.FullName)
                While Not streamRead.EndOfStream
                    Dim uneLigne As String
                    uneLigne = streamRead.ReadLine
                    If firstRow Then
                        Dim fonctionDeChaine As String()
                        fonctionDeChaine = uneLigne.Split(",")
                        DataGridView1.ColumnCount = fonctionDeChaine.Length
                        For i As Integer = 0 To fonctionDeChaine.Length - 1
                            DataGridView1.Columns(i).Name = fonctionDeChaine(i)
                        Next
                        firstRow = False
                        dimension = fonctionDeChaine.Length
                        Continue While
                    End If
                    Dim matchDate = Date.Parse(uneLigne.Substring(0, 8))
                    If matchDate < Date.Today Then
                        Continue While
                    End If
                    Dim row As String()
                    row = New String(dimension) {}
                    Dim idx As Integer
                    idx = -1
                    While uneLigne.Contains(",")
                        idx = idx + 1
                        Dim unChamp As String
                        ' the location field is the only field that is surrounded by double quote
                        ' we could encouter an escaped double quote
                        If uneLigne.StartsWith("""") Then
                            Dim endIndexOfLocationField = uneLigne.LastIndexOf("""")
                            unChamp = uneLigne.Substring(1, endIndexOfLocationField)
                            uneLigne = uneLigne.Substring(endIndexOfLocationField + 2)
                        Else
                            unChamp = uneLigne.Substring(0, uneLigne.IndexOf(","))
                            uneLigne = uneLigne.Substring(uneLigne.IndexOf(",") + 1)
                        End If
                        row.SetValue(unChamp, idx)
                    End While
                    DataGridView1.Rows.Add(row)
                End While
            End Using
        Loop

    End Sub
    Sub DownloadScheduleByTeamNumber(ByVal team_id)
        Dim url As String
        Dim filepath As String
        filepath = Path.GetTempPath() & My.Application.Info.AssemblyName & "\\" & team_id & "_regular_season_schedule.csv"
        If Not (My.Computer.FileSystem.FileExists(filepath)) Then
            url = "http://www.ticketing-client.com/ticketing-client/csv/EventTicketPromotionPrice.tiksrv?team_id=" & team_id & "&display_in=singlegame&ticket_category=Tickets&site_section=Default&sub_category=Default&leave_empty_games=true&event_type=T"
            My.Computer.Network.DownloadFile(url, filepath)
        End If
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Me.WindowState = WindowState.Maximized

        Dim NoTeams = New NumberedTeamNames("", 0)
        ComboBox1.Items.Add(NoTeams)
        Dim allTeams = New NumberedTeamNames("All", 1)
        ComboBox1.Items.Add(allTeams)

        teamsDictionary = New Dictionary(Of Integer, String)

        Dim teams = New List(Of String)()
        Dim team_id As Integer
        Dim tempDir As DirectoryInfo
        tempDir = Directory.CreateDirectory(Path.GetTempPath() & My.Application.Info.AssemblyName)
        ' Validity of the following non consecutive 30 mlb team numbers is subject to change at ticketing-client.com administrators discretion
        For team_id = 108 To 121
            DownloadScheduleByTeamNumber(team_id)
        Next
        For team_id = 133 To 147
            DownloadScheduleByTeamNumber(team_id)
        Next
        team_id = 158
        DownloadScheduleByTeamNumber(team_id)

        Dim files As IEnumerable
        files = tempDir.EnumerateFiles()
        Dim enumerator As IEnumerator
        enumerator = files.GetEnumerator()
        Do While enumerator.MoveNext()
            Dim sched As FileInfo
            sched = enumerator.Current
            Dim stopIndex = sched.Name.IndexOf("_")
            Dim ticketingTeamNumber = Integer.Parse(sched.Name.Substring(0, stopIndex))
            Dim firstRow As Boolean = True
            ' In the very worst case scenario, 
            ' a team calendar would book two 5 games series against the same opponent away and home, hence ten
            Dim bufferToKnowWhoseTeamIsThatCalendar = 10
            Dim upToBuffer = 0
            Dim calTeamNotDetermined = True
            Dim seasonStartTeamSubDictionnary As Dictionary(Of String, Integer)
            seasonStartTeamSubDictionnary = New Dictionary(Of String, Integer)
            Using streamRead As New StreamReader(sched.FullName)
                While Not streamRead.EndOfStream
                    Dim uneLigne As String
                    uneLigne = streamRead.ReadLine
                    If firstRow Then
                        firstRow = False
                        Continue While
                    End If

                    Dim matchDate = Date.Parse(uneLigne.Substring(0, 8))
                    If matchDate < DateTimePicker1.Value Then
                        Continue While
                    End If

                    upToBuffer = upToBuffer + 1

                    If upToBuffer = bufferToKnowWhoseTeamIsThatCalendar Then
                        ' Here the most beats the leasts
                        Dim maxOcc = 0
                        Dim currentTeamName As String
                        For Each k As String In seasonStartTeamSubDictionnary.Keys()
                            If seasonStartTeamSubDictionnary.Item(k) > maxOcc Then
                                currentTeamName = k
                                maxOcc = seasonStartTeamSubDictionnary.Item(k)
                            End If
                        Next
                        teamsDictionary.Add(ticketingTeamNumber, currentTeamName)
                        calTeamNotDetermined = False
                    End If
                    If Not calTeamNotDetermined Then
                        Exit While
                    End If
                    Dim idx As Integer
                    idx = 0
                    Dim SubjectFieldNotReached = True

                    While uneLigne.Contains(",") And SubjectFieldNotReached
                        idx = idx + 1
                        If idx = 4 Then
                            ' we are facing the Subject field
                            Dim aTeam = uneLigne.Substring(0, uneLigne.IndexOf(" at "))
                            If Not teams.Contains(aTeam) Then
                                teams.Add(aTeam)
                            End If
                            If seasonStartTeamSubDictionnary.ContainsKey(aTeam) Then
                                seasonStartTeamSubDictionnary.Item(aTeam) = seasonStartTeamSubDictionnary.Item(aTeam) + 1
                            Else
                                seasonStartTeamSubDictionnary.Add(aTeam, 1)
                            End If
                            SubjectFieldNotReached = True
                        End If
                        uneLigne = uneLigne.Substring(uneLigne.IndexOf(",") + 1)
                    End While
                End While
            End Using
        Loop
        For Each teamNumber As Integer In teamsDictionary.Keys
            Dim currentTeam = New NumberedTeamNames(teamsDictionary.Item(teamNumber), teamNumber)
            ComboBox1.Items.Add(currentTeam)
        Next
        ComboBox1.SelectedIndex = 0
    End Sub

    Private Sub DataGridView1_Paint(sender As Object, e As PaintEventArgs) Handles DataGridView1.Paint
        If DataGridView1.Rows.Count = 0 Then
            TextRenderer.DrawText(e.Graphics, "~ ticketing-client.com.", DataGridView1.Font, DataGridView1.ClientRectangle, DataGridView1.ForeColor, DataGridView1.BackgroundColor, TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter)
        End If
    End Sub
End Class
