The chain storage system is responsible for storing chains in memory, loading them, and finding nearby merge points.
It is important to separate the two types of chains. Chains, represented in code by the AbstractChain abstraction, represent links in a chain consisting of chains.
The entire chain from the first to the last block can consist of several AbstractChain connected to each other. 
This approach allows you to separate hash sets, lists, and other information, thereby reducing the likelihood of collisions.In addition, 
dividing chains into smaller chains allows you to simplify the transition from one chain to another.