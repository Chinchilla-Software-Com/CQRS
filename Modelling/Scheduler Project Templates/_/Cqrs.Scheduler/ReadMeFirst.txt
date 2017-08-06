One of the hardest things to do in modern programming is scheduled or batch processing.
This micro-service scheduler allows you to trigger any number of actions/operations
based on a schedule with 15 minute increments.
One of the most challenging aspects of scheduling is dealing with different time-zones.
This is made even more challenging by the fact not all time-zones are offset from UTC
by exactly 60 minutes. There are some time-zones that, in addition to being one or more hours offset,
are also 15, 30 and even 45 minutes offset from UTC.

It runs in several different modes, each can be turned on or off:
* Run one or more actions/operations when each time-zone is at mid-night.
* Run one or more actions/operations when each time-zone is on the hour.
* Run one or more actions/operations when each time-zone is 15 minutes past the hour.
* Run one or more actions/operations when each time-zone is 30 minutes past the hour.
* Run one or more actions/operations when each time-zone is 45 minutes past the hour.

This solution comes with one sample action/operation called an EventHandler.
It is located in the EventHandlers folder of the SampleReport project.
This sample action sends an email advising the recipient as each time-zone reaches midnight.
The email address the email gets sent to is configured in the app.config along with the SMTP settings.

When testing or running this locally, always start the SampleReport application first.