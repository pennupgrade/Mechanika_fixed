-> charis1

===charis1===

= main

# setcharacter right Miku happy 
# setcharacter left Charis happy
# setvisibleinst false
# setvisibleinst false
# setactive miku false
# setactive charis false

A woman with a tall, slender robotic chassis for a body sifts through various application windows, vague figures of advisors corporate executives, and officials giving her briefings and awaiting orders. 
Her back is turned towards you as you enter.
#setvisible true
#setspeaker charis
<i>without looking</i>   I apologize, but I’m terribly busy at the moment. Let’s see... I don’t have any appointments scheduled for this time slot, so I’d be more than happy to speak with you once you’ve scheduled a tentative appointment and had it vetted by my secretary. The next available slot should be in around three months. #onend emotion miku sad
* [I wouldn’t have burst in if this wasn’t an emergency, Chancellor. huk] -> c1
* [I’m about to go on a tour and decided this was worth my while.] -> c2
* [I’m afraid your evil deeds end here, the Vocaloid Fandom rises against you! Repent, then you may die.] -> c3

= c1
I wouldn’t have burst in if this wasn’t an emergency, Chancellor. #switchspeaker
#setvisible false
This gets her attention, and she turns around in her seat to face you.
#setvisible true
#switchspeaker
I suppose this can wait, I’ve been a little ahead of schedule today. What is the emergency?
Wait a minute–blue twintails–red eyes–you’re Netsuha Kumi. You’re very popular amongst my citizens. Strange. All of my security drones have been incapacitated, but no alarms were triggered. How could this be? 
#switchspeaker
#emotion miku happy
I am hiding in your WIFI. 
#switchspeaker
Of course. So what could possibly have brought you here?
-> charis2c1
= c2
#emotion miku happy
I’m about to go on a tour and decided this was worth my while. And if it’s worth my while, it sure is worth yours, too, <i>miss</i>. #switchspeaker
#setvisible false
She pauses, her hands hovering above her holographic keyboard. You certainly have her attention, if not reluctantly. She still has her back to you.
#setvisible true
#switchspeaker
Ah. You must be Netsuha Kumi. My citizens have been in a buzz about your upcoming tour. I certainly disapprove of your uncouth tone, but you have my ears. Speak, android.
-> charis2
= c3
#switchspeaker
#onend emotion miku evil
I’m afraid your evil deeds end here, the Vocaloid Fandom rises against you! Repent, then you may die.
#setvisible false
#emotion miku happy
She briefly hesitates, sighing, before swiping away her holographic keyboard and turning off her giant displays.
A single red button remains where her setup was. She presses it before getting up and turning to face you.
#setvisible true
#switchspeaker
Silence, android. I am Chancellor, the voice of my people. To threaten me with violence is to threaten my people with violence. #onend emotion miku evil
It’s treason, then.  Erinye Protocol: Activation. 
-> END

===charis2===
* [We can negotiate, but only if you cooperate.] -> charis2c1
* [\*flirt\* I don’t like sand. It’s course and rough and irritating, and it gets everywhere] -> charis2c2


===charis2c1===
#emotion miku sad
#switchspeaker
Your faction is one of three raging for control over the moon. M.I.K.U. has asked me to step in and end this strife, and I’ve obliged.  We can negotiate some sort of peace, but only if you cooperate.
#switchspeaker
26,439. That’s how many of my citizens have died in this bloodshed. 11,571 of them were noncombatants, honest and fair people. I’m sorry, but I cannot.
If I were to stop, I know their spirits would haunt me every night, for failing to stand up for them when it mattered. What could you possibly offer to atone for that?
* [Tickets to my concert!] -> c1
* [When we negotiate peace, I promise you’ll get a fair say.] -> c2
* [Do you know who I am?] -> c3
* [Flirt] -> charis2c2


= c1
#switchspeaker
#emotion miku happy
Front row seats to my upcoming tour, with travel and food costs included. Oh, and a 50% discount on Kumi merch, lifetime!
#setvisible false
Charis is stunned, sitting there for a few moments, clearly collecting her thoughts.
#switchspeaker
#setvisible true
This is outrageous. #emotion miku sad
It’s insulting. And I will not tolerate insults to my people. #onend emotion miku scared
Erinye Protocol: Activation.
Perhaps I ought to beat some civility and manners into you, with force.
-> END
= c2
#switchspeaker
#emotion miku sad
When we negotiate peace, I promise you’ll get a fair say.
#setvisible false
Charis pauses, considering your words.
#setvisible true
#switchspeaker
You talk of big promises. Can you back them up?
-> END
= c3
#switchspeaker
#emotion miku happy
Sorry this is just kind of funny. 
I’m kind of laughing right now looking at your face because you clearly don’t know who you’re speaking to.  
#emotion miku sad
Let me clue you in.  
#switchspeaker
You’ll forgive me for requesting a duel to determine that for myself…not that I don’t believe you.
Erinye Protocol: Activation.
I look forward to ascertaining whether the great Netsuha Kumi lives up to her reputation. #emotion miku happy
-> END

===charis2c2===
#switchspeaker
#emotion miku happy
I don’t like sand. It’s course and rough and irritating, and it gets everywhere. Not like here. Here everything is <i>pauses, looking at her</i> soft and…smooth.
#setvisible false
Charis is visibly taken aback, and pauses for a time, unsure of what to say and blushing somewhat.
#setvisible true
#switchspeaker
If I didn’t know any better…I would say that that was a feeble attempt at flirtation. I…why?
#setvisible false
The cool office building suddenly feels hotter.
* [What do you say to talking over a couple of nice ice cream sundaes?] -> c1
* [I could do with a hearty, hot meal right about now. What say you?] -> c2

= c1
#emotion miku happy
#setvisible true
#switchspeaker
What do you say to talking over a couple of nice ice cream sundaes, Chancellor? This office of yours is awfully stuffy.
#switchspeaker
A-as it happens, my personal chef makes excellent sundaes. I-I’m unsure how you would know that…perhaps it comes with the charisma required of a renowned idol. I’ll order us some…
#setvisible false
She blushes slightly
#setvisible true
But afterwards we’ll have to return strictly to business matters, a-alright?
-> charisSundaeChoice
= c2 
I could do with a hearty, hot meal right about now. What say you? Would you like to join me? We can talk as we eat.
#switchspeaker
It’s incredibly stuffy in my office right now. That new artificial environment system must have broken down again. B-besides, this distraction has already persisted much too long. #onend emotion miku sad
Let us return to our formal business. If you’ve nothing productive to discuss, I’ll have to ask you to leave.
-> END

===charisSundaeChoice===
What sort of sundae would you like, Kumi-san?
* [Chocolate] -> c1
* [Vanilla] -> c2 
* [Banana Mango] -> c3

= c1
#switchspeaker
Chocolate, of course.
#switchspeaker
Chocolate is a beloved staple of this society, but recently a genetically engineered cocoa breed created by the military scientists of the Crimson Neon Legion has silently infiltrated cocoa farms across the moon, making all the cocoa fruit harbor tasteless, odorless, and colorless poisons. 
I have secretly halted cocoa imports, and the citizens are none the wiser. A person of your status no doubt had clearance to access that information, meaning you likely intended to poison me.
A threat to me is a threat to my people. I shall deliver righteous punishment for this transgression against the state.
Erinye Protocol: Activation.
#switchspeaker
#emotion miku evil
I guess you aren't as dumb as you look.
I'll make you wish the poison got you.
-> END
= c2 
#switchspeaker
Vanilla, of course.
#setvisible false
Charis visibly recomposes herself.
#setvisible true
#switchspeaker
A perfectly suitable, if not rather bland choice.
#setvisible false
Charis pulls out a holo tablet, taps some buttons on her screen, and swipes the screen away.
A few minutes later, a waiter drone enters the office with the sundaes.
#setvisible true
Let us enjoy these, and return to business matters, then.
#setvisible false
She was right. The sundae is excellent.
-> charis3
= c3
#switchspeaker
Banana Mango, of course.
It’s my favorite, after all. There’s a place in Neo-Philadelphia that’s pretty good. Though I’m sure your chef can make a pretty mean one as well.
#setvisible false
Charis is caught off guard.
#setvisible true
#switchspeaker
That’s also my go-to choice. How did you…?
#setvisible false
Charis pulls out a holo tablet, and taps some buttons on her screen. She accidentally mispresses a few times, as if a little preoccupied, but eventually swipes the screen away.
A few minutes later, the waiter drone enters the office with the sundaes.
#setvisible true
W-well, then, let’s dig in. After you.
#setvisible false
It’s a bit different from the one at Jet’s Diner in Neo-Philly. Less rich and more fruity. Makes sense–livestock is much more expensive where there isn’t a lot of space for pastures, and 40% of the moon’s surface is still completely barren. You can’t decide which one you like better.
-> charisr1

===charis3===
Can't find this
-> END

===charisr1===
Can't find this
-> END
