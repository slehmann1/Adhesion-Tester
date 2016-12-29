%This is an application developed by Samuel Lehmann in the summer of 2016
%for the lab of Dr. Dan Sameoto. This application takes in data produced by
%the radial trial function of its partner C# application (AdhesionTest) and
%produces a variety of figures. If you wish to contact Samuel Lehmann, you
%may be able to find him at: sjlehman@ualberta.ca


%The entry point
function adhesionVisualizer()
    %Reset the environment
    clc;
    clear;
    
    defaultMeshSize=1;
    defaultRadialGridIncrement=10;
    
    %Find the file
    [fileName,pathName] = uigetfile('.csv','Select the save file to create a plot for');
    fileID = fopen(strcat(pathName,fileName));
    
    %Check whether it is a valid file
    line = fgetl(fileID);
    
    if(~strcmp(line, 'Trial type: Radial Trial '))
        uiwait(errordlg('Unsupported trial type or incorrect file format','File Error','modal'));
        adhesionVisualizer();
    end
    
    %Read the file for data
    index=1;
    dataStruct=readFileIteration(fileID);
    while(~isempty(fieldnames(dataStruct)))
        data(index)=dataStruct;
        dataStruct=readFileIteration(fileID);
        index=index+1;
    end
    
    %Select the plot type
    choice = chooseDialog();
    
    counter=1;
    
    %Set up the arrays for plotting
    for i=1:length(data)
        switch choice
            case 1
                value = data(i).dragDistance;
            case 2
                value = data(i).incomingVerticalAngle;
            case 3
                value = data(i).outgoingVerticalAngle;
        end
        for j=1:length(data(i).dataArray)
            x(counter)=cosd(data(i).dataArray(j,1))*value;
            y(counter)=sind(data(i).dataArray(j,1))*value;
            z(counter)=data(i).dataArray(j,3);
            counter=counter+1;
        end
    end
    
    switch choice
        case 1
            zTitle ='Drag Distance (?m)';
        case 2
            zTitle ='Incoming Vertical Angle (°)';
        case 3
            zTitle ='Outgoing Vertical Angle (°)';
    end
    
    %Plot the data
    drawGraphs(x,y,z,zTitle,defaultMeshSize,defaultRadialGridIncrement);
end

%plots a circle centered at the origin where r is the radius
function plotCircle(r,color)
    x=0;
    y=0;
    angleStep=0.001;
    ang=0:angleStep:2*pi;
    xp=r*cos(ang);
    yp=r*sin(ang);
    plot(x+xp,y+yp,'Color',color);
end

%Adds a radial legend to the plot in the xy plane, in the same process
%hiding the cartesian grid in the xy plane
function addRadialLegend(maxSize,radialGridIncrement,axisTitle)
    textXMargin=1;
    %Both of the title text margins are multiplicative
    titleTextXMargin=-1;
    titleTextYMargin=0;
    axisColor=[0.1,0.1,0.1];
    ax=gca;
    %  ax.GridColor=axisColor;
    yTextPosition=0;
    set(gca,'xgrid','off','ygrid','off','zgrid','on' );
    set(gca,'XTickLabel',{' '}, 'YTickLabel',{' '});
    set(gca,'xtick',[], 'ytick',[])
    set(gca, 'ZColor', axisColor);
    set(ax,'xcolor','w','ycolor','w');
    set(gca,'GridAlpha',1)
    alpha 0.8;
    for radius=radialGridIncrement:radialGridIncrement:maxSize
        hold on;
        plotCircle(radius,axisColor);
        t=text(radius+textXMargin,yTextPosition,num2str(radius));
        t.FontWeight='bold';
        t.Color=axisColor;
    end
    
    %Add the axis title to the end
    t=text(maxSize*titleTextXMargin,maxSize*titleTextYMargin,axisTitle);
    t.FontWeight='bold';
    t.Color=axisColor;
    
end

%Draw the graphs, using x,y, and z data, the titles for the variable axis
%and the desired grid size. Will create figures if none present already,
%otherwise will use the ones made beforehand
function drawGraphs(x,y,z,axisTitle,gridSize,radialGridIncrement)
    set(gcf, 'Renderer', 'opengl');
    zTitle = 'Adhesion (mN)';
    transparency=0.6;
    %Matlab doesn't have a ternary operator
    maxSize = max(x);
    if max(y)>maxSize
        maxSize=max(y);
    end
    
    try
        
        cameraPosition = [150 150 50];
        
        %Delaunay (Triangular) approximation plot
        figure(1);
        clf;
        tri = delaunay(x,y);
        plot=trisurf(tri,x,y,z);
        addRadialLegend(maxSize,radialGridIncrement,axisTitle);
        xlabel(axisTitle);
        ylabel(axisTitle);
        zlabel(zTitle);
        set(gca,'CameraPosition', cameraPosition);
        set(plot,'EdgeAlpha',transparency);
        colorbar EastOutside;
        title('delaunay plot');
        
        %Scatterplot
        figure(2);
        clf;
        plot=scatter3(x,y,z,'filled');
        alpha(plot,transparency);
        addRadialLegend(maxSize,radialGridIncrement,axisTitle);
        xlabel(axisTitle);
        ylabel(axisTitle);
        zlabel(zTitle);
        set(gca,'CameraPosition',cameraPosition)
        
        %Convex hull 1
        figure(3);
        clf;
        ix=convhull(x,y,z);
        hold on;
        plot=mesh(x(ix),y(ix),z(ix));
        set(plot,'EdgeAlpha',transparency);
        addRadialLegend(maxSize,radialGridIncrement,axisTitle);
        xlabel(axisTitle);
        ylabel(axisTitle);
        zlabel(zTitle);
        title('convex hull');
        set(gca,'CameraPosition',cameraPosition)
        shading interp
        colorbar EastOutside
        
        %Convex hull 2
        figure(4);
        clf;
        ix=convhull(x,y,z);
        hold on;
        plot=surf(x(ix),y(ix),z(ix));
        set(plot,'EdgeAlpha',transparency);
        addRadialLegend(maxSize,radialGridIncrement,axisTitle);
        xlabel(axisTitle);
        ylabel(axisTitle);
        zlabel(zTitle);
        title('convex hull');
        set(gca,'CameraPosition',cameraPosition)
        shading interp
        colorbar EastOutside
        
        %Convex hull overlaid onto scatterplot
        figure(5);
        clf;
        ix=convhull(x,y,z);
        plot=scatter3(x,y,z,'filled');
        alpha(plot,transparency);
        hold on;
        %Doesn't support transparency, love you Matlab
        plot3(x(ix(:,3)),y(ix(:,3)),z(ix(:,3)));
        addRadialLegend(maxSize,radialGridIncrement,axisTitle);
        xlabel(axisTitle);
        ylabel(axisTitle);
        zlabel(zTitle);
        title('convex hull');
        set(gca,'CameraPosition',cameraPosition)
        dimx = xlim;
        dimy=ylim;
        
        %Rectangular approximation plot
        figure(6);
        clf;
        [xi,yi] = meshgrid(dimx(1):gridSize:dimx(2),dimy(1):gridSize:dimy(2));
        zi = griddata(x,y,z,xi,yi);
        plot=surf(xi,yi,zi);
        set(plot,'EdgeAlpha',transparency);
        addRadialLegend(maxSize,radialGridIncrement,axisTitle);
        xlabel(axisTitle);
        ylabel(axisTitle);
        zlabel(zTitle);
        set(gca,'CameraPosition',cameraPosition)
        colorbar EastOutside
        title('Rectangular grid');
        
        %Add the Ui to the final figure to allow for updating of the meshgrid
        
        % meshGrid inputBox
        meshGridSlider = uicontrol('Style', 'edit',...
            'Position', [300 20 80 20],...
            'String',num2str(gridSize));
        
        % label
        uicontrol('Style','text',...
            'Position',[230 20 80 20],...
            'String','Mesh grid size');
        
        %radialGrid inputBox
        radialGridSlider = uicontrol('Style', 'edit',...
            'Position', [100 20 80 20],...
            'String',num2str(radialGridIncrement));
        
        % label
        uicontrol('Style','text',...
            'Position',[30 20 80 20],...
            'String','Radial grid size');
        
        % button to update the sliders
        uicontrol('Style','pushbutton',...
            'Position',[400 20 80 20],...
            'String','Update', 'Callback', @inputCallback);
    catch e
        disp(e.identifier);
        errordlg('Cannot graph this data, please ensure there is variation in the feature you chose to graph');
        close all;
    end
    %The callback
    function inputCallback(~,~)
        try
            meshGridVal = str2double(meshGridSlider.String);
            radialGridVal = str2double(radialGridSlider.String);
            if meshGridVal<=0 ||radialGridVal<=0
                throw(MException('adhesionVisualizer:invalidInput', 'Invalid Input'));
            end
        catch
            errordlg('Invalid Input');
            return;
        end
        
        %Redraw the graphs if the input is valid
        drawGraphs(x,y,z,axisTitle,meshGridVal,radialGridVal)
    end
end

%Create a UI dialog allowing the user to select the characteristic to graph
%Returns: 1 = drag distance, 2 = incoming vertical angle, 3= outgoing vertical angle
function choice = chooseDialog
    
    d = dialog('Position',[300 300 250 150],'Name','Select One');
    uicontrol('Parent',d,...
        'Style','text',...
        'Position',[20 80 210 40],...
        'String','Select a feature to plot');
    
    uicontrol('Parent',d,...
        'Style','popup',...
        'Position',[75 70 100 25],...
        'String',{'Drag distance';'Incoming vertical angle';'Outgoing vertical angle'},...
        'Callback',@popup_callback);
    
    uicontrol('Parent',d,...
        'Position',[89 20 70 25],...
        'String','Continue',...
        'Callback','delete(gcf)');
    
    choice = 1;
    
    % Wait for d to close before running to completion
    uiwait(d);
    
    function popup_callback(popup,~)
        idx = popup.Value;
        choice = idx;
    end
end

%Read an iteration from the file returning its data in a struct, returns an
%empty struct if there is no new iteration to read
function [dataStruct]= readFileIteration(fileID)
    try
        %Search for the next iteration
        line='';
        while (~strcmp(line, 'New Trial Iteration: '))
            line=fgetl(fileID);
            if(feof(fileID))
                dataStruct=struct();
                return;
            end
        end
        %Read the trial characteristics
        dragString = fgetl(fileID);
        dragString = dragString(16:end);
        dragNum=str2double(dragString);
        angleString = fgetl(fileID);
        angleString = angleString(26:end);
        incomingAngleNum = str2double(angleString);
        angleString = fgetl(fileID);
        angleString = angleString(26:end);
        outgoingAngleNum = str2double(angleString);
        fgetl(fileID);
        
        %Collect data
        data='';
        line=strcat(fgetl(fileID),';');
        data=strcat(data,line);
        while (~(strcmp(line,';')||feof(fileID)))
            line=strcat(fgetl(fileID),';');
            data=strcat(data,line);
        end
        
        %Convert the data to an array
        dataArray = eval( [ '[', data, ']' ] );
        
        %Create the struct and return
        dataStruct.incomingVerticalAngle =incomingAngleNum;
        dataStruct.outgoingVerticalAngle =outgoingAngleNum;
        dataStruct.dragDistance=dragNum;
        dataStruct.dataArray=dataArray;
        return;
    catch
        dataStruct=struct();
        return;
    end
end
