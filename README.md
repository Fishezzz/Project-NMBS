# Project NMBS

## Summary ##
This is my final project for the subject [L OOP 2](http://onderwijsaanbod.vives-zuid.be/syllabi/n/V3M025N.htm#activetab=doelstellingen_idp32096) (Object Oriented Programming) at my school.<br>
I got this subject in the first semester of the 2nd year of my [professional bachelor in Electronics-ICT](https://www.vives.be/nl/opleidingen/iwt/ict).

This project is mainly focused on working with and visualizing the [public data](http://www.belgianrail.be/nl/klantendienst/faq/public-data.aspx?cat=wat) of the [NMBS](https://www.belgiantrain.be/nl).<br>
In the application you have 4 different tabs where you can view/do different this:
* __Route planner__: Here you can select 2 stations, a date and a time of the day. Then you can hit the "Query" button to search the possible trips, if any are available. If there are trips available, they will display in a ListView. __This is done with DataBinding.__ The departure time and platform at the station of departure and the arrival time and platform at the station of arrival will be visible.
* __Trip viewer__: Here you can select a station and view what trips go through the station, as well as the arrival time at the final station of the trip and the last date when the trip is serviced.
* __Real time__: Here you can select a station to see if there are any realtime updates available. Mind that there are not always updates available for every station at any time. Any available results are shown in a ListView. Each result shows the name of the first and the last station of the trip, as well as the date of the trip and the arrival time  When double-clicking on a search result, a new window will open with the tripId as title. The same info as the search result in the ListView will be shown followed by a ListView with every Stop that has updated information. This information contains the name op the stop, the updated arrival and departure time. There will almost always be delays given in the realtime feed, as the NMBS does not deliver info about trains stopping or passing through stations even if they do not have a delay. Note that both stations that get serviced on the trip and stations that are passed through may appear in the results.
* __Route finder__: Here you can select a station and view all the routes that service the selected station. The result is the short name of the route (ex: _IC_, _P_) follow by the long name of the route (ex: _Oostend -- Welkenraedt_). When you double-click a result, a new window will open with the routeId as title. The same info as the search result in the ListView will be shown followed by a ListView with the name of every Stop that is serviced (stations that get passed through are not shown), the latitude and longtitude of the station.

All the ListView's are being filled with DataBinding. This happens in the following way:<br>
`DisplayMemberBinding="{Binding Item1}"` in the XAML file,<br>
`lvResultRouteplanner.ItemsSource = itemSource;` in the C# file.<br>
The variable `itemSource` that fills the ListView where, for example, the search results for the searched station is shown, is a `List<Tuple<string, Stop>>`. Because a `Tuple<T1,T2>` always has it's 'properties' named `Item1`, `Item2`, etc. you can make use of this as an advantage and use these fixed names in the DataBinding.

There is a lot of processing that needs to be done when starting the application, and when executing a query. Therefore it takes close to 10 seconds to start up the application. But keep in mind there are over 700.000 lines which must be read. Thus over 700.000 objects that need to be created. Nonetheless I managed to keep the query times under 4 seconds.

## Project requirements and self-evaluation ##
### Project requirements ###
>The requirements for the project, given by the teacher.

1. The project consists of a realization of a personal idea, executed in a C # WPF Application.
2. The app is crash-resistant through well-considered exception handling.
3. There is at least one exception that you have created yourself (derived from `ApplicationException`).
4. There is a clear GUI (groupboxes, not too many and not too few controls, ...).
5. The code is made up of logical blocks (methods) that stimulate code reuse.
6. By using constantes, lengths of collections, relative paths, ... the code can easily be adapted to changed needs.
7. At least one piece of data is stored in an external file.
8. We work with self-written classes and classes derived from them.
9. At least one of the above classes will be added to the project via a DLL file.
10. Polymorphism is used in at least one location.
11. In at least one place in your app, use LINQ to Objects.
12. There is a link to a MySQL database or you fetch JSON objects that you convert through the NewtonSoft library.
13. There is at least one extension method that is used correctly.
14. The app meets the expectations (operation).
15. Deadline for submitting the project has been respected.

### Self-evaluation ###
>Evaluation my own work, whether I fulfilled the project requirements.

- [x] 1. The project consists of a realization of a personal idea, executed in a C # WPF Application.<br>
_Check!_
- [x] 2. The app is crash-resistant through well-considered exception handling.<br>
_Check!_
- [x] 3. There is at least one exception that you have created yourself (derived from `ApplicationException`).<br>
_Check!_
- [x] 4. There is a clear GUI (groupboxes, not too many and not too few controls, ...).<br>
_Check!_
- [x] 5. The code is made up of logical blocks (methods) that stimulate code reuse.<br>
_Check!_
- [x] 6. By using constantes, lengths of collections, relative paths, ... the code can easily be adapted to changed needs.<br>
_Check!_
- [x] 7. At least one piece of data is stored in an external file.<br>
_Check! I read from several files.<br>
The teacher never said whether this should be reading and/or writing, so I consider this requirement fulfilled._
- [x] 8. We work with self-written classes and derived classes from these classes.<br>
_Check!_
- [x] 9. At least one of the above classes will be added to the project via a DLL file.<br>
_Check!_
- [x] 10. Polymorphism is used in at least one location.<br>
_Check!_
- [x] 11. In at least one place in your app, use LINQ to Objects.<br>
_Check!_
- [ ] 12. There is a link to a MySQL database or you fetch JSON objects that you convert through the NewtonSoft library.<br>
_I don't use a database or JSON. But I use something else, which is made specifically for the topic of my project.<br>
It is called GTFS or General Transit Feed Specification._
- [x] 13. There is at least one extension method that is used correctly.<br>
_Check!_
- [x] 14. The app meets the expectations (operation).<br>
_Check!_
- [ ] 15. Deadline for submitting the project has been respected.<br>
_I have failed tremendously in attending the presentation moment at school. I have set my alarm, but it has not gone off._

- - - -

 &not; Fishezzz &nbsp; &laquo; 19 November 2018 &raquo;
