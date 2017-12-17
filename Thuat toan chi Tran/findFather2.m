% function [orderedJunc, orderedLines] = findFather2(newCoor, st, S)
clc; clear all; close all;
% 
% addpath('Phu');
% % 
% load newCoor
load st
load S
load I

% st = start;
Skel = S;

% newCoor = [0 1; 1 0; 2 3; 3 4; 2 3; 1 0; 0 1; 0 5];
% newCoor = [3 3; 2 3; 2 2; 2 3; 0 0;1 1; 1 1; 1 2];

newCoor = [];
figure, 
imshow(I); title('I');
hold on;
for i=1:length(Skel)
    k = Skel{i};
    disp(k);
    plot(k(:,2),k(:,1),'-');
    hold on; 
    
    % taking the first and the last nodes
    fi = k(1,:);
    la = k(end, :);
    if (fi==la)
        continue;
    end
    newCoor = [newCoor; fi; la];
    disp(newCoor);
end
hold off;

figure,
plot(newCoor(:,2), newCoor(:,1), '*');

sumCoor = sum(newCoor,2);
[M, idx] = min(sumCoor);
st = idx; 


disp('Start Point: ');
disp(newCoor(st,:));

% loai nhung diem trung nhau
[C, ia, ic] = unique(newCoor, 'rows', 'stable');

lev = 0;
% start = 3;

% khoi tao father and level cho start
start = ic(st);
level = -ones(size(C,1),1);
level(start) = lev;

father = -ones(size(C,1),1);
father(start) = 0;

lev = lev + 1;
Ric = ic;

orderedLines = [];
count = 0;

while (~isempty(start))
    disp('start = ');
    disp(start);
    
    % find next
    next = [];
    for i=1:length(start)
        pos = find(Ric==start(i));
        if (isempty(pos))
            continue;
        end
        
        Ric(pos) = -1;
        
        % find next
        temp = mod(pos,2);
        temp(temp==0) = -1;
        child = pos+temp;
        
        father(Ric(child)) = start(i);
        next = [next Ric(child)'];
        Ric(child) = -1;
        
        % add in orderedLines
        k = round(pos/2);
        orderedLines = [orderedLines S(k)];
    end
    
    % dangerous
    if (isempty(next) && ~isequal(Ric,-ones(size(Ric))))
        % find neighbor point
        p = C(start,:);
        A = repmat(p,size(Ric,1),1);
        
        B = sum((newCoor-A).^2,2);
        [err,vt] = sort(B);
        
        next = vt(2);
        father(next) = start;
    end
    
    level(next) = lev;
    
    lev = lev+1;
    start = next;
    
end

[orderedJunc, idx] = sortrows([C, father, level], 4);
% disp('ordered Junctions');
% disp(orderedJunc);

% chuyen father
father2 = zeros(size(father));
A = orderedJunc(:,3);
for i=2:length(father2)
    k = find(idx==A(i), 1);
    father2(i) = k;
end

% ordered Junctions
orderedJunc(:,3) = [];
orderedJunc = [orderedJunc father2];

% t = array2table(orderedJunc,'VariableNames',{'x','y','level','father'});
% disp(t);
% end









