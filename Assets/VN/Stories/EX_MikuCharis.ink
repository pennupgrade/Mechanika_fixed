-> charis1

===charis1===

= main

# setcharacter right Miku happy 
# setcharacter left Charis happy
# setvisible miku false
# setvisible charis false
# setactive miku false
# setactive charis false

A woman with a tall, slender robotic chassis for a body sifts through various application windows, vague figures of advisors corporate executives, and officials giving her briefings and awaiting orders. 
Her back is turned towards you as you enter.

#setvisible miku true
#setvisible charis true
#setspeaker charis

\*without looking\* \n I apologize, but I’m terribly busy at the moment. Let’s see... I don’t have any appointments scheduled for this time slot, so I’d be more than happy to speak with you once you’ve scheduled a tentative appointment and had it vetted by my secretary. The next available slot should be in around three months.

#emotion miku sad
* [I wouldn’t have burst in if this wasn’t an emergency, Chancellor.] -> c1
* [I’m about to go on a tour and decided this was worth my while (unprofessional)] -> c2
* [The only thing that’ll be on your schedule in three months is physical therapy.] -> c2

= c1
I wouldn’t have burst in if this wasn’t an emergency, Chancellor. #switchspeaker
This gets her attention, and she turns around in her seat to face you. #switchspeaker
I suppose this can wait, I’ve been a little ahead of schedule today. What is the emergency? #switchspeaker
Wait a minute–you’re Netsuha Kumi. You’re very popular amongst my citizens. Strange. All of my security drones have been incapacitated, but no alarms were triggered. How could this be? #switchspeaker
I am hiding in your WIFI. #switchspeaker
I see. So what could possibly have brought you here? #switchspeaker
-> charis2
= c2
I’m about to go on a tour and decided this was worth my while. And if it’s worth my while, it sure as heck is worth yours, too, lady. #switchspeaker
#setactive miku false
She pauses, her hands hovering above her holographic keyboard. You certainly have her attention, if not reluctantly. She still has her back to you.
#setactive charis true
Ah. You must be Netsuha Kumi. My citizens have been in a buzz about your upcoming tour. I certainly disapprove of your uncouth tone, but you have my ears. Speak.
-> charis2
= c3
 The only thing that’ll be on your schedule in three months is physical therapy. I’m about to mess you and that tacky hunk of metal you call a body the hell up!
She briefly hesitates, sighing, before swiping away her holographic keyboard and turning off her giant displays.
A single red button remains where her setup was. She presses it before getting up and turning to face you.
Charis: I am Chancellor, the voice of my people. To threaten me with violence is to threaten my people with violence.
Charis: It’s treason, then.
Charis: Erinye Protocol: Activation. 
-> END

===charis2===
-> END