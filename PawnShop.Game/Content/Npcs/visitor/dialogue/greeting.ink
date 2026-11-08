// Prototype content, not established story canon. Edit in VS Code; preview with ./npc.ps1 play -Id visitor.
VAR asked_about_radio = false
-> greeting

=== greeting ===
Is this the shop that gives old things another chance? I have a radio with a very strong opinion about silence. # line: visitor.greeting # speaker: visitor # emotion: worried
-> topics

=== topics ===
What would you like to know? # line: visitor.topics # speaker: visitor
+ {not asked_about_radio} [What happened to the radio? # id: ask_radio]
    ~ asked_about_radio = true
    -> radio
+ [What brings you to the shop? # id: ask_shop]
    -> shop
+ {asked_about_radio} [Does the radio do anything at all? # id: follow_up]
    -> follow_up
+ [Good luck with the radio. Goodbye. # id: goodbye]
    -> farewell

=== radio ===
It used to play music. Now it only clicks, which is either a fault or a very small drummer. # line: visitor.radio # speaker: visitor # emotion: confident
I am keeping it for now. This is just a chat; there is no repair job or reward attached. # line: visitor.no_job # speaker: visitor
-> topics

=== shop ===
I like a place that sees a lamp with one leg and thinks: there is still a business opportunity here. # line: visitor.shop # speaker: visitor # emotion: pleased
-> topics

=== follow_up ===
It holds a door open beautifully. I am trying not to let that affect my expectations. # line: visitor.follow_up # speaker: visitor # emotion: pleased
-> topics

=== farewell ===
Thank you. If it starts a band, you will be the first to know. # line: visitor.farewell # speaker: visitor # emotion: pleased
-> END
