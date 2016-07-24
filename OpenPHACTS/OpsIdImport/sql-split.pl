$counter = 0;
while(<>) {
	print;
	print "GO\n" if $counter % 1000 == 0;
	$counter++;
}
0;