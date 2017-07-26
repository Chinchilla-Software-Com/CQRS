#!/bin/sh

/opt/doxygen/bin/doxygen ./cqrs.doxyconf
python -m coverxygen --xml-dir ./$1/xml/ --output ./doc-coverage.info --src-dir ../../Framework/ --scope public,protected --kind enum,varible,function,class,struct,define,file,page
#python -m coverxygen --xml-dir ./2.2/xml/ --output ./doc-coverage.info --src-dir ../../Framework/ --scope public,protected --kind enum,varible,function,class,struct,define,file,page
lcov --summary doc-coverage.info
genhtml --no-function-coverage --no-branch-coverage ./doc-coverage.info -o ./$1/coverage-report/
#genhtml --no-function-coverage --no-branch-coverage ./doc-coverage.info -o ./2.2/coverage-report/

for f in $(find ./$1/coverage-report/ -name '*.html')
do
	sed -i 's/<title>LCOV - doc-coverage\.info<\/title>/<title>Documentation Coverage Report<\/title>/' $f
	sed -i 's/LCOV - code coverage report/Documentation Coverage Report/' $f
	sed -i 's/<td class="headerItem">Test:<\/td>/<td class="headerItem">Version:<\/td>/' $f
	sed -i 's/<td class="headerValue">doc-coverage.info<\/td>/<td class="headerValue">2.2<\/td>/' $f
	sed -i 's/<td class="headerItem">Lines:<\/td>/<td class="headerItem">Artefacts:<\/td>/' $f
	sed -i 's/Line Coverage/Documentation Coverage/' $f
done

